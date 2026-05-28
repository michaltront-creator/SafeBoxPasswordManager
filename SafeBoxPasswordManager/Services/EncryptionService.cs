using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SafeBoxPasswordManager.Services
{
    public class EncryptionService
    {
        private readonly byte[] _key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
        private readonly byte[] _iv = Encoding.UTF8.GetBytes("1234567890123456");

        public string Encrypt(string plainText)
        {
            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using MemoryStream memoryStream = new MemoryStream();
            using CryptoStream cryptoStream = new CryptoStream(
                memoryStream,
                aes.CreateEncryptor(),
                CryptoStreamMode.Write);

            using StreamWriter writer = new StreamWriter(cryptoStream);
            writer.Write(plainText);
            writer.Close();

            byte[] encryptedBytes = memoryStream.ToArray();

            return Convert.ToBase64String(encryptedBytes);
        }

        public string Decrypt(string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);

            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using MemoryStream memoryStream = new MemoryStream(encryptedBytes);
            using CryptoStream cryptoStream = new CryptoStream(
                memoryStream,
                aes.CreateDecryptor(),
                CryptoStreamMode.Read);

            using StreamReader reader = new StreamReader(cryptoStream);

            return reader.ReadToEnd();
        }
    }
}
