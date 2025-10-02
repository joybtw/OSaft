using System;
using System.Security.Cryptography;
using System.Text;

namespace OSaft.usersystem
{
    public class User
    {
        public string Name { get; private set; }
        public uint Uid { get; private set; }
        public bool IsRoot { get; private set; }
        private string passwordHash { get; set; }

        private static readonly int MaxNameLength = 32;

        public User(string newName, string password, bool root, uint id)
        {
            if (string.IsNullOrWhiteSpace(newName) || newName.Length > MaxNameLength)
                throw new ArgumentException("Invalid username");

            this.Name = newName;
            this.passwordHash = ComputeSHA256(password);
            this.IsRoot = root;
            this.Uid = id;
        }

        private string ComputeSHA256(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(hashBytes);
            }
        }

        public bool VerifyPassword(string password)
        {
            string attemptHash = ComputeSHA256(password);
            return attemptHash == this.passwordHash;
        }
    }
}
