using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SafeBoxPasswordManager.Services
{
    public class TotpService
    {
        private const int SecretSize = 20;
        private const int TimeStepSeconds = 30;
        private const int CodeDigits = 6;

        private readonly string _secretFilePath = "totp_secret.dat";

        public bool IsTotpConfigured()
        {
            return File.Exists(_secretFilePath);
        }

        public string CreateSecret()
        {
            byte[] secretBytes = RandomNumberGenerator.GetBytes(SecretSize);
            string secret = ToBase32(secretBytes);

            File.WriteAllText(_secretFilePath, secret);

            return secret;
        }

        public string GetSecret()
        {
            if (!File.Exists(_secretFilePath))
            {
                return string.Empty;
            }

            return File.ReadAllText(_secretFilePath).Trim();
        }

        public bool VerifyCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return false;
            }

            string secret = GetSecret();

            if (string.IsNullOrWhiteSpace(secret))
            {
                return false;
            }

            long currentStep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / TimeStepSeconds;

            for (int i = -1; i <= 1; i++)
            {
                string expectedCode = GenerateCode(secret, currentStep + i);

                if (expectedCode == code.Trim())
                {
                    return true;
                }
            }

            return false;
        }

        public string GetManualSetupKey()
        {
            return GetSecret();
        }

        private string GenerateCode(string base32Secret, long timeStep)
        {
            byte[] key = FromBase32(base32Secret);
            byte[] counter = BitConverter.GetBytes(timeStep);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(counter);
            }

            using HMACSHA1 hmac = new HMACSHA1(key);
            byte[] hash = hmac.ComputeHash(counter);

            int offset = hash[^1] & 0x0F;

            int binaryCode =
                ((hash[offset] & 0x7F) << 24) |
                ((hash[offset + 1] & 0xFF) << 16) |
                ((hash[offset + 2] & 0xFF) << 8) |
                (hash[offset + 3] & 0xFF);

            int otp = binaryCode % (int)Math.Pow(10, CodeDigits);

            return otp.ToString().PadLeft(CodeDigits, '0');
        }

        private string ToBase32(byte[] bytes)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

            StringBuilder result = new StringBuilder();
            int buffer = 0;
            int bitsLeft = 0;

            foreach (byte b in bytes)
            {
                buffer = (buffer << 8) | b;
                bitsLeft += 8;

                while (bitsLeft >= 5)
                {
                    int index = (buffer >> (bitsLeft - 5)) & 31;
                    bitsLeft -= 5;
                    result.Append(alphabet[index]);
                }
            }

            if (bitsLeft > 0)
            {
                int index = (buffer << (5 - bitsLeft)) & 31;
                result.Append(alphabet[index]);
            }

            return result.ToString();
        }

        private byte[] FromBase32(string base32)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

            string cleaned = base32.Trim().Replace(" ", "").Replace("=", "").ToUpperInvariant();

            MemoryStream stream = new MemoryStream();
            int buffer = 0;
            int bitsLeft = 0;

            foreach (char c in cleaned)
            {
                int value = alphabet.IndexOf(c);

                if (value < 0)
                {
                    continue;
                }

                buffer = (buffer << 5) | value;
                bitsLeft += 5;

                if (bitsLeft >= 8)
                {
                    stream.WriteByte((byte)((buffer >> (bitsLeft - 8)) & 255));
                    bitsLeft -= 8;
                }
            }

            return stream.ToArray();
        }
    }
}
