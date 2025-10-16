using System;
using System.Text;
using System.Xml;

namespace OSaft.usersystem
{
    public class User
    {
        public string Name { get; private set; }
        public uint Uid { get; private set; }
        public bool IsSuperuser { get; private set; }
        public string PasswordHash { get; private set; }
        public string Salt { get; private set; }

        private static readonly int MaxNameLength = 32;
        private static readonly int SaltSize = 16; // 16 Bytes Salt

        // --- Linear Congruential Generator ---
        private static uint seed = 123456789;
        private static uint NextUInt()
        {
            seed = (1103515245 * seed + 12345) & 0x7FFFFFFF;
            return seed;
        }

        private static byte[] GenerateSaltBytes(int size)
        {
            byte[] saltBytes = new byte[size];
            for (int i = 0; i < size; i++)
                saltBytes[i] = (byte)(NextUInt() % 256);
            return saltBytes;
        }

        private static string BytesToHex(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            int b;
            for (int i = 0; i < bytes.Length; i++)
            {
                b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }
            return new string(c);
        }

        private static string GenerateSalt()
        {   
            return BytesToHex(GenerateSaltBytes(SaltSize));
        }

        // --- FNV-1a Hash (statt SHA256) ---
        private static string ComputeFNV1a(string input)
        {
            const uint fnvPrime = 16777619;
            const uint offsetBasis = 2166136261;
            uint hash = offsetBasis;

            byte[] data = Encoding.UTF8.GetBytes(input);
            for (int i = 0; i < data.Length; i++)
            {
                hash ^= data[i];
                hash *= fnvPrime;
            }

            // Hex-String zurückgeben
            return hash.ToString("X8");
        }

        public static string HashPassword(string password, string salt)
        {
            return ComputeFNV1a(password + salt);
        }

        public User(string newName, string password, string salt, bool sudo, uint id, bool hashed)
        {
            if (string.IsNullOrWhiteSpace(newName) || newName.Length > MaxNameLength)
                throw new ArgumentException("Invalid username", nameof(newName));

            this.Name = newName;
            if (salt == null) { this.Salt = GenerateSalt(); }
            else { this.Salt = salt; }
            if (hashed == false) { this.PasswordHash = HashPassword(password, this.Salt); }
            else { this.PasswordHash = password; }
            this.IsSuperuser = sudo;
            this.Uid = id;
        }
    }
}
