using System;
using System.IO;

namespace PlatinumPOS.Services
{
    /// <summary>
    /// A deployment marker used for client-side "new version available" detection.
    /// The value is the newest last-write time across every file in the deployed app
    /// directory (binaries AND static assets like CSS/JS), so it changes whenever a new
    /// build is deployed — including static-only changes that don't recompile the DLL —
    /// but NOT on a plain service restart with the same files, avoiding false prompts.
    /// Exposed to the browser via the <c>/app-version</c> endpoint and the
    /// <c>app-version</c> meta tag; <c>wwwroot/version-check.js</c> compares them.
    /// </summary>
    public static class AppVersion
    {
        public static readonly string Current = Compute();

        private static string Compute()
        {
            try
            {
                var dir = AppContext.BaseDirectory;
                long newest = 0;
                foreach (var f in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
                {
                    var ticks = File.GetLastWriteTimeUtc(f).Ticks;
                    if (ticks > newest) newest = ticks;
                }
                if (newest > 0) return newest.ToString();
            }
            catch
            {
                // fall through to the per-process fallback
            }

            // Fallback (e.g. unreadable directory): a per-process id.
            return Guid.NewGuid().ToString("N");
        }
    }
}
