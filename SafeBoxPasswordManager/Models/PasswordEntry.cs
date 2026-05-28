using System;

namespace SafeBoxPasswordManager.Models
{
    public class PasswordEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string ServiceName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public override string ToString()
        {
            return $"{ServiceName} - {Username}";
        }
    }
}
