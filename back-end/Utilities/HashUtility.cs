using static System.Net.WebRequestMethods;
using System.Security.Cryptography;
using System.Text;

namespace back_end.Utilities
{
    public static class HashUtility
    {
        public static string HashByBCrypt(string input, int workFactor)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentNullException("Input must not be null or empty");
            }
            return BCrypt.Net.BCrypt.HashPassword(input, workFactor: workFactor);
        }
        public static string HashBySHA256(string input, string secrectKey)
        {
            if (string.IsNullOrEmpty(input))
            {
                throw new ArgumentException("Input must not null or empty");
            }
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secrectKey));
            var otpBytes = Encoding.UTF8.GetBytes(input);
            var hashedBytes = hmac.ComputeHash(otpBytes);
            return Convert.ToBase64String(hashedBytes);
        }
        public static bool Verify(
            string input,
            string hashedInput,
            bool isBCrypt,
            string? shaSecrectKey = default
        )
        {
            if (isBCrypt)
            {
                return BCrypt.Net.BCrypt.Verify(
                    input,
                    hashedInput
                );
            }
            if (shaSecrectKey == null)
            {
                throw new ArgumentNullException("SHA Secrect key is null or empty!");
            }
            string computedHash = HashBySHA256(input, shaSecrectKey);

            return CryptographicOperations.FixedTimeEquals
            (
                Convert.FromBase64String(computedHash),
                Convert.FromBase64String(hashedInput)
            );
        }
    }
}
