using Microsoft.EntityFrameworkCore;
using PlatinumPOS.Data;
using PlatinumPOS.Models;
using System.Threading.Tasks;

namespace PlatinumPOS.Services
{
    public class PosSettingsService
    {
        private readonly IDbContextFactory<PosDbContext> _dbFactory;

        public PosSettingsService(IDbContextFactory<PosDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<string> GetSettingAsync(string key, string defaultValue)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            var setting = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
            return setting?.Value ?? defaultValue;
        }

        public async Task SetSettingAsync(string key, string value)
        {
            using var db = await _dbFactory.CreateDbContextAsync();
            var setting = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting == null)
            {
                db.SystemSettings.Add(new SystemSetting { Key = key, Value = value });
            }
            else
            {
                setting.Value = value;
                db.SystemSettings.Update(setting);
            }
            await db.SaveChangesAsync();
        }

        public async Task<string> GetStoreNameAsync() => await GetSettingAsync("StoreName", "Platinum Brink Cafe & Deli");
        public async Task<string> GetCurrencySymbolAsync() => await GetSettingAsync("CurrencySymbol", "R");
        public async Task<decimal> GetTaxRateAsync()
        {
            var str = await GetSettingAsync("TaxRate", "15");
            return decimal.TryParse(str, out var val) ? val : 15m;
        }
    }
}
