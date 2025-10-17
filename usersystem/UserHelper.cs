using OSaft.filesystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OSaft.usersystem
{
    public static class UserHelper
    {
        internal readonly static Dictionary<string, User> users = new();
        private static string currentUser = "";
        public static string CurrentUser
        {
            get => currentUser;
            set => currentUser = value;
        }

        public static void Initialize()
        {

            if (File.Exists(@"0:\system\root_init.sys"))
            {
                Console.WriteLine("System already initialized. Proceeding to login...");
                Console.WriteLine("--------------------------------");
                LoginUser();
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
                Console.ForegroundColor = ConsoleColor.Black;
                password = Console.ReadLine();
                Console.ResetColor();
            } while (password == null);

            User rootUser = new(name, password, null, true, 0, false);


            if (!Directory.Exists("system"))
                FileSystem.CreateDirectory("system");

            if (!Directory.Exists("users"))
                FileSystem.CreateDirectory("users");

            FileSystem.CreateDirectory($@"users\{name}");
            FileSystem.CreateFile(@"system\root_init.sys");

            UserDatabase.SaveAllUsers(new List<User> { rootUser });

            Console.WriteLine("\nSudo user created successfully.");
            Console.WriteLine("--------------------------------");

            // Nach erfolgreicher Einrichtung direkt zum Login
            LoginUser();
        }

        private static void LoginUser()
        {
            var users = UserDatabase.LoadAllUsers();
            if (users.Count == 0)
            {
                Console.WriteLine("No users found. Please reset filesystem (Skript on Discord)(to be changed).");
                return;
            }

            string name = "";

            for (;;)
            {
                while (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("Skip Login with 'dev'");
                    Console.Write("Enter user name: ");
                    name = Console.ReadLine();
                    if (name == "dev") { return; }
                }
                var found = users.Find(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

                if (found == null)
                {
                    Console.WriteLine("User not found.");
                    LoginUser();
                    return;
                }
                Console.Write("Enter password: ");
                Console.ForegroundColor = ConsoleColor.Black;
                string password = Console.ReadLine();
                Console.ResetColor();

                if ((found.PasswordHash == User.HashPassword(password, found.Salt)))
                {
                    Console.WriteLine($"Login successful. Welcome {found.Name}!");
                    Console.WriteLine(found.IsSuperuser ? "You are logged in as a superuser." : "Standard user session started.");
                    Console.WriteLine("--------------------------------");
                    currentUser = found.Name;
                    return;
                }
                else
                {
                    Console.WriteLine("Incorrect password. Access denied.");
                    name = "";
                    Console.WriteLine("--------------------------------");
                }

            }
        }
    }

    public static class UserDatabase
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

