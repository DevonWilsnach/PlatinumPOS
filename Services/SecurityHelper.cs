using System;
using System.Security.Cryptography;
using System.Text;

namespace PlatinumPOS.Services
{
    public static class SecurityHelper
    {
        private const string Salt = "PlatinumPOS_Salt_2026!";

        public static string HashPin(string pin)
        {
            if (string.IsNullOrEmpty(pin)) return string.Empty;
            
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(pin + Salt);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hashBytes);
        }

        public static bool VerifyPin(string pin, string hash)
        {
            if (string.IsNullOrEmpty(pin) || string.IsNullOrEmpty(hash)) return false;
            var inputHash = HashPin(pin);
            return string.Equals(inputHash, hash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
