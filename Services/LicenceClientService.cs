using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PlatinumPOS.Data;
using PlatinumPOS.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PlatinumPOS.Services
{
    public class LicenceStatusDto
    {
        public bool IsLicensed { get; set; }
        public string StatusLabel { get; set; } = "Unknown";
        public string StatusMessage { get; set; } = string.Empty;
        public string Footprint { get; set; } = string.Empty;
        public DateTime? PeriodTo { get; set; }
        public string LicenceType { get; set; } = string.Empty;
    }

    public class LicenceClientService
    {
        private readonly IDbContextFactory<PosDbContext> _dbFactory;
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        // PlatinumAuth product bucket for this app.
        private const string ProductCode = "platinumpos";

        public LicenceClientService(IDbContextFactory<PosDbContext> dbFactory, HttpClient http, IConfiguration config)
        {
            _dbFactory = dbFactory;
            _http = http;
            _config = config;
        }

        public async Task<string> GetFootprintAsync()
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            var setting = await db.SystemSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == "DeviceFootprint");
            if (setting != null && !string.IsNullOrEmpty(setting.Value))
            {
                return setting.Value;
            }

            var raw = Guid.NewGuid().ToString("N").ToUpperInvariant();
            var fp = "POS-" + string.Join("-", Enumerable.Range(0, 7).Select(i => raw.Substring(i * 4, 4)));
            
            try
            {
                var newSetting = new SystemSetting { Key = "DeviceFootprint", Value = fp };
                db.SystemSettings.Add(newSetting);
                await db.SaveChangesAsync();
                return fp;
            }
            catch (DbUpdateException)
            {
                using var db2 = await _dbFactory.CreateDbContextAsync();
                var existing = await db2.SystemSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == "DeviceFootprint");
                if (existing != null && !string.IsNullOrEmpty(existing.Value))
                {
                    return existing.Value;
                }
                throw;
            }
        }

        public async Task<string> GetAuthBaseUrlAsync()
        {
            // Config wins so production is pinned to auth.platinumbrink.com regardless of any saved DB value.
            var fromConfig = _config["Licence:AuthBaseUrl"];
            if (!string.IsNullOrWhiteSpace(fromConfig)) return fromConfig;
            using var db = await _dbFactory.CreateDbContextAsync();
            var setting = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == "W8AuthBaseUrl");
            return setting?.Value ?? "https://auth.platinumbrink.com";
        }

        public async Task SetAuthBaseUrlAsync(string url)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            var setting = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == "W8AuthBaseUrl");
            if (setting == null)
            {
                db.SystemSettings.Add(new SystemSetting { Key = "W8AuthBaseUrl", Value = url });
            }
            else
            {
                setting.Value = url;
                db.SystemSettings.Update(setting);
            }
            await db.SaveChangesAsync();
        }

        public async Task<LicenceStatusDto> GetStatusAsync()
        {
            var footprint = await GetFootprintAsync();
            using var db = await _dbFactory.CreateDbContextAsync();
            
            var cacheSetting = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == "LicenceCache");
            if (cacheSetting == null || string.IsNullOrEmpty(cacheSetting.Value))
            {
                return new LicenceStatusDto
                {
                    IsLicensed = false,
                    StatusLabel = "Unlicensed",
                    Footprint = footprint,
                    StatusMessage = "No license found. Connect to PlatinumAuth to register."
                };
            }

            try
            {
                var cached = JsonSerializer.Deserialize<LicenceCacheData>(cacheSetting.Value);
                if (cached == null)
                {
                    return new LicenceStatusDto
                    {
                        IsLicensed = false,
                        StatusLabel = "Unlicensed",
                        Footprint = footprint,
                        StatusMessage = "Invalid license cache. Sync required."
                    };
                }

                bool expired = cached.PeriodTo <= DateTime.UtcNow;
                bool valid = cached.IsValid && !expired;

                return new LicenceStatusDto
                {
                    IsLicensed = valid,
                    StatusLabel = !valid ? "Expired" : "Licensed",
                    StatusMessage = !cached.IsValid ? "License is disabled by administrator."
                        : expired ? $"License expired on {cached.PeriodTo.ToLocalTime():dd MMM yyyy}."
                        : $"License valid until {cached.PeriodTo.ToLocalTime():dd MMM yyyy}.",
                    Footprint = footprint,
                    PeriodTo = cached.PeriodTo,
                    LicenceType = cached.LicenceType
                };
            }
            catch
            {
                return new LicenceStatusDto
                {
                    IsLicensed = false,
                    StatusLabel = "Error",
                    Footprint = footprint,
                    StatusMessage = "Error reading cached license. Sync required."
                };
            }
        }

        public async Task<bool> IsLicensedAsync()
        {
            var status = await GetStatusAsync();
            return status.IsLicensed;
        }

        public async Task<bool> RefreshAsync()
        {
            try
            {
                var footprint = await GetFootprintAsync();
                var baseUrl = await GetAuthBaseUrlAsync();
                
                // Product-aware route so PlatinumAuth buckets this device under PlatinumPOS.
                var url = $"{baseUrl.TrimEnd('/')}/api/v1/ApplicationModuleLicences/acquire/{Uri.EscapeDataString(ProductCode)}/{Uri.EscapeDataString(footprint)}";

                // Empty POST body
                var response = await _http.PostAsync(url, new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var json = await response.Content.ReadAsStringAsync();
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var result = JsonSerializer.Deserialize<AcquireResponse>(json, opts);
                var record = result?.ApplicationModuleLicences?.PageResult?.FirstOrDefault();

                if (record == null)
                {
                    return false;
                }

                var cache = new LicenceCacheData
                {
                    IsValid = record.IsActive,
                    PeriodFrom = record.PeriodFrom,
                    PeriodTo = record.PeriodTo,
                    LicenceType = record.LicenceType ?? "Full",
                    SyncTime = DateTime.UtcNow
                };

                using var db = await _dbFactory.CreateDbContextAsync();
                var cacheSetting = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == "LicenceCache");
                var cacheJson = JsonSerializer.Serialize(cache);

                if (cacheSetting == null)
                {
                    db.SystemSettings.Add(new SystemSetting { Key = "LicenceCache", Value = cacheJson });
                }
                else
                {
                    cacheSetting.Value = cacheJson;
                    db.SystemSettings.Update(cacheSetting);
                }

                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private class LicenceCacheData
        {
            public bool IsValid { get; set; }
            public DateTime PeriodFrom { get; set; }
            public DateTime PeriodTo { get; set; }
            public string LicenceType { get; set; } = string.Empty;
            public DateTime SyncTime { get; set; }
        }

        private class AcquireResponse
        {
            [JsonPropertyName("applicationModuleLicences")]
            public LicencePageResult? ApplicationModuleLicences { get; set; }
        }

        private class LicencePageResult
        {
            [JsonPropertyName("pageResult")]
            public System.Collections.Generic.List<LicenceRecord>? PageResult { get; set; }
        }

        private class LicenceRecord
        {
            [JsonPropertyName("isActive")] public bool IsActive { get; set; }
            [JsonPropertyName("periodFrom")] public DateTime PeriodFrom { get; set; }
            [JsonPropertyName("periodTo")] public DateTime PeriodTo { get; set; }
            [JsonPropertyName("licenceType")] public string? LicenceType { get; set; }
        }
    }
}
