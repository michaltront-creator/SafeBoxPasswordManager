using System;
using System.IO;
using System.Security.Cryptography;

namespace SafeBoxPasswordManager.Services
{
    public class MasterPasswordService
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100_000;

        private readonly string _filePath = "master_password.dat";

        public bool IsMasterPasswordSet()
        {
            return File.Exists(_filePath);
        }

        public void CreateMasterPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = HashPassword(password, salt);

            byte[] fileData = new byte[SaltSize + HashSize];

            Buffer.BlockCopy(salt, 0, fileData, 0, SaltSize);
            Buffer.BlockCopy(hash, 0, fileData, SaltSize, HashSize);

            File.WriteAllBytes(_filePath, fileData);
        }

        public bool VerifyMasterPassword(string password)
        {
            if (!File.Exists(_filePath))
            {
                return false;
            }

            byte[] fileData = File.ReadAllBytes(_filePath);

            if (fileData.Length != SaltSize + HashSize)
            {
                return false;
            }

            byte[] salt = new byte[SaltSize];
            byte[] savedHash = new byte[HashSize];

            Buffer.BlockCopy(fileData, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(fileData, SaltSize, savedHash, 0, HashSize);

            byte[] enteredHash = HashPassword(password, salt);

            return CryptographicOperations.FixedTimeEquals(savedHash, enteredHash);
        }

        private byte[] HashPassword(string password, byte[] salt)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);
        }
    }
}
