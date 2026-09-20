using System.Security.Cryptography;
using System.Text;

namespace back_end.Utilities
{
    public static class OtpUtility
    {
        public static string Generate(int len)
        {
            if (len < 1)
            {
                throw new ArgumentException("Length must be at least 1");
            }
            var number = RandomNumberGenerator.GetInt32(0, 1_000_000);

            return number.ToString($"D{len}");
        }
    }
}
