using OSaft.filesystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace OSaft.usersystem
{
    public class UserHelper
    {
        internal readonly static Dictionary<string, User> users = new();
        public static void Initialize()
        {
            if (File.Exists(@"0:\system\root_init.sys")) { return; }

            Console.Write("Enter user name: ");
            String name = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(name))
            {
                Console.Write("Enter user name: ");
                name = Console.ReadLine();
            }

            Console.Write("Enter password: ");
            String password = Console.ReadLine();
            while (password == null)
            {
                Console.Write("Enter a password for user " + name + "");
                password = Console.ReadLine();
            }

            User root_u = new(name, password, true, 0);
            if (!Directory.Exists("system"))
            {
                FileSystem.CreateDirectory("system");
            }
            FileSystem.CreateDirectory("users");
            FileSystem.CreateDirectory(@"users\" + name);
            FileSystem.CreateFile(@"system\root_init.sys");
            User_Database.SaveUser(root_u);
            Console.WriteLine("Sudo user created");
            Console.WriteLine("Your name is: " + root_u.Name);
            //Console.WriteLine(root_u.PasswordHash);
            Console.WriteLine("--------------------------------");
            return;
        }
    }


    public class Login
    {
        public static void LoginUser()
        {
            Console.Write("Enter user name: ");
            String name = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(name))
            {
                Console.Write("Enter user name: ");
                name = Console.ReadLine();
            }

            // ... Login a user
        }
    }

    public class User_Database
    {
        private static readonly string user_db_path = @"system\users.db";
        private  static readonly byte _key = 0x5A;

        public static void DBInit()
        {
            if (!File.Exists(user_db_path)) 
            {
                FileSystem.CreateFile(user_db_path);
            }
            return;
        }

        public static void SaveUser(User new_u) 
        {

            string content = $"{new_u.Name}|{new_u.PasswordHash}|{new_u.IsSuperuser}|{new_u.Uid}"; 
            byte[] data = Encoding.ASCII.GetBytes(content);

            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= _key;
            }
            byte checksum = 0;
            foreach (var b in data)
                checksum ^= b;

            byte[] finalData = new byte[data.Length + 1];
            data.CopyTo(finalData, 0);
            finalData[^1] = checksum; // Checksumme im letzten Element speichern

            DBInit();
            /* testing failed
             
            var file = File.OpenWrite(user_db_path);
            file.Seek(0, SeekOrigin.End);
            file.Write(finalData, 0, finalData.Length);
            return;
            */
        }
        public (string Username, string PasswordHash)? LoadUser()
        {
            var file = File.OpenRead(user_db_path);
            byte[] data = new byte[file.Length];
            file.Read(data, 0, data.Length);

            byte checksum = data[^1];
            byte computed = 0;
            for (int i = 0; i < data.Length - 1; i++)
                computed ^= data[i];

            if (checksum != computed)
            {
                Console.WriteLine("Error while reading! UserDB has been changed without authorization!");
                return null;
            }

            // XOR entschlüsseln
            for (int i = 0; i < data.Length - 1; i++)
                data[i] ^= _key;

            string content = Encoding.ASCII.GetString(data, 0, data.Length - 1);
            var parts = content.Split('|');
            return (parts[0], parts[1]);
        }
    }
} 