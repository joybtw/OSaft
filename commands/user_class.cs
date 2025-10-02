using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace OSaft.users
{

    public class User
    {
        string name{ get; set; }
        uint uid { get; set; }
        bool is_root { get; set; }  
        //int gid; // future use
        string password_hash { get; set; }
        private static readonly int max_nlen = 32;

        public User()
        {
            if (name.Length > max_nlen) throw new Exception("Username too long");
        }
        public User(string new_name, string password, bool root, uint id)
        {
            this.name = new_name;
            // this.password_hash = HashPassword(password);
            this.is_root = root;
            this.uid = id;
        }

    }
}