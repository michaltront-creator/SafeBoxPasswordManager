using SafeBoxPasswordManager.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace SafeBoxPasswordManager.Services
{
    public class PasswordStorageService
    {
        private readonly string _filePath = "password_entries.json";
        private readonly EncryptionService _encryptionService = new EncryptionService();


        public List<PasswordEntry> LoadEntries()
        {
            if (!File.Exists(_filePath))
            {
                return new List<PasswordEntry>();
            }

            string encryptedJson = File.ReadAllText(_filePath);

            if (string.IsNullOrWhiteSpace(encryptedJson))
            {
                return new List<PasswordEntry>();
            }

            try
            {
                string json = _encryptionService.Decrypt(encryptedJson);

                List<PasswordEntry>? entries = JsonSerializer.Deserialize<List<PasswordEntry>>(json);

                return entries ?? new List<PasswordEntry>();
            }
            catch
            {
                return new List<PasswordEntry>();
            }
        }


        public void SaveEntries(List<PasswordEntry> entries)
        {
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(entries, options);
            string encryptedJson = _encryptionService.Encrypt(json);

            File.WriteAllText(_filePath, encryptedJson);
        }

    }
}
