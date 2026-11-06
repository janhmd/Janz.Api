using System.Security.Cryptography;
using System.Text;

namespace Service.Helpers
{
    public enum EmailCodeResult
    {
        Valid,
        Wrong,
        Expired,
        Locked
    }

    // Qeydiyyatda emailə gedən 4 rəqəmli kod: bazada kodun özü yox, hash-i, bitmə vaxtı və səhv cəhdlərin sayı saxlanılır
    public static class EmailCode
    {
        public const int MaxAttempts = 5;
        public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);

        public static string NewCode() => RandomNumberGenerator.GetInt32(0, 10000).ToString("D4");

        public static string Pack(string userId, string code, DateTime nowUtc)
        {
            return $"{Hash(userId, code)}|{(nowUtc + Lifetime).Ticks}|0";
        }

        public static (EmailCodeResult Result, string Stored) Check(string? stored, string userId, string? code, DateTime nowUtc)
        {
            var parts = (stored ?? string.Empty).Split('|');
            if (parts.Length != 3 || !long.TryParse(parts[1], out var expires) || !int.TryParse(parts[2], out var attempts))
            {
                return (EmailCodeResult.Expired, string.Empty);
            }

            if (nowUtc.Ticks > expires)
            {
                return (EmailCodeResult.Expired, stored!);
            }

            if (attempts >= MaxAttempts)
            {
                return (EmailCodeResult.Locked, stored!);
            }

            var given = Encoding.UTF8.GetBytes(Hash(userId, code?.Trim() ?? string.Empty));
            var real = Encoding.UTF8.GetBytes(parts[0]);
            if (CryptographicOperations.FixedTimeEquals(given, real))
            {
                return (EmailCodeResult.Valid, stored!);
            }

            attempts++;
            var updated = $"{parts[0]}|{parts[1]}|{attempts}";

            return (attempts >= MaxAttempts ? EmailCodeResult.Locked : EmailCodeResult.Wrong, updated);
        }

        private static string Hash(string userId, string code)
        {
            return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(userId + ":" + code)));
        }
    }
}
