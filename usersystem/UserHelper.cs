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
       
            if (File.Exists(@"0:\system\root_init.sys"))
            {
                Console.WriteLine("System already initialized. Proceeding to login...");
                Console.WriteLine("--------------------------------");
                Login.LoginUser();
                return;
            }

            // Erster Systemstart -> Root-User erstellen
            Console.WriteLine("This is the first boot of OSaft. Please create a user.");

            string name;
            do
            {
                Console.Write("Enter user name: ");
                name = Console.ReadLine();
            } while (string.IsNullOrWhiteSpace(name));

            string password;
            do
            {
                Console.Write("Enter password: ");
                password = Console.ReadLine();
            } while (password==null);

            User rootUser = new(name, password, null , true, 0, false);
           Console.WriteLine (rootUser.Salt);

            if (!Directory.Exists("system"))
                FileSystem.CreateDirectory("system");

            if (!Directory.Exists("users"))
                FileSystem.CreateDirectory("users");

            FileSystem.CreateDirectory($@"users\{name}");
            FileSystem.CreateFile(@"system\root_init.sys");

            User_Database.SaveAllUsers(new List<User> { rootUser });

            Console.WriteLine("\nSudo user created successfully.");
            Console.WriteLine("--------------------------------");

            // Nach erfolgreicher Einrichtung direkt zum Login
            Login.LoginUser();
        }
    }

    public class Login
    {
        public static void LoginUser()
        {
            var users = User_Database.LoadAllUsers();
            if (users.Count == 0)
            {
                Console.WriteLine("No users found. Please reset filesystem (Skript on Discord)(to be changed).");
                return;
            }
             
            string name = "";
            while (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Skip Login with 'dev'");
                Console.Write("Enter user name: ");
                name = Console.ReadLine();  
                if (name == "dev") { return; }
            }

            Console.Write("Enter password: ");
            string password = Console.ReadLine();

            var found = users.Find(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (found == null)
            {
                Console.WriteLine("User not found.");
                LoginUser(); // Retry login 
            }

            if ((found.PasswordHash == User.HashPassword(password, found.Salt)))
            {
                Console.WriteLine($"Login successful. Welcome {found.Name}!");
                Console.WriteLine(found.IsSuperuser ? "You are logged in as a superuser." : "Standard user session started.");
                Console.WriteLine("--------------------------------");
            }
            else
            {
                Console.WriteLine("Incorrect password. Access denied.");
                Console.WriteLine(found.PasswordHash+" "+User.HashPassword(password, found.Salt) +" " + found.Salt);
                Console.WriteLine("--------------------------------");
                LoginUser(); // Retry login
            }
        }
    }

    public static class User_Database
    {
        private static readonly string user_db_path = @"\system\users.db";

        public static void DBInit()
        {
            if (!File.Exists(user_db_path))
            {
                FileSystem.CreateFile(user_db_path);
            }
        }

        public static void SaveAllUsers(List<User> users)
        {
            DBInit();

            StringBuilder sb = new();
            foreach (var u in users)
            {
                Console.WriteLine(u.Salt+ "SaveUser");
                sb.Append(u.Name).Append('|')
                  .Append(u.PasswordHash).Append('|')
                  .Append(u.Salt).Append('|')
                  .Append(u.IsSuperuser ? "1" : "0").Append('|')    
                  .Append(u.Uid).Append('\n');
            }

            FileSystem.WriteFile(user_db_path, sb.ToString());
            Console.WriteLine("[UserDB] Saved " + users.Count + " user(s)");
        }

        public static List<User> LoadAllUsers()
        {
            var list = new List<User>();

            string content = FileSystem.ReadFile(user_db_path);
            if (string.IsNullOrWhiteSpace(content))
            {
                Console.WriteLine("[UserDB] No content found in users.db");
                return list;
            }

            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length < 5) continue;

                try
                {
                    string name = parts[0];
                    string hash = parts[1];
                    string salt = parts[2];
                    bool isSuperuser = parts[3] == "1";
                    uint uid = uint.Parse(parts[4]);

                Console.WriteLine(salt + "LoadUser");
                    list.Add(new User(name, hash, salt, isSuperuser, uid, true));
                   
                }
                catch (Exception e)
                {
                    Console.WriteLine("[UserDB] Error parsing user line: " + e.Message);
                }
            }

            Console.WriteLine("[UserDB] Loaded " + list.Count + " user(s)");
            return list;
        }
    }


}

