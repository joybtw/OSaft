using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace OSaft.users
{

    public class User
    {
        string name { get; set; }
        uint uid { get; set; }
        bool is_root { get; set; }
        string password_hash { get; set; }
        private static readonly int max_nlen = 32;

        public User()
        {
            if (name.Length > max_nlen) throw new Exception("Username too long");
        }

        public User(string new_name, string password, bool root, uint id)
        {
            this.name = new_name;
            this.password_hash = ComputeHMACSHA256(password);
            this.is_root = root;
            this.uid = id;
        }

        private string ComputeHMACSHA256(string input)
        {
            using (var hmac = new HMACSHA256())
            {
                byte[] hashBytes = hmac.ComputeHash(System.Text.Encoding.UTF32.GetBytes(input));
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}