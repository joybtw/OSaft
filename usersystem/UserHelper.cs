
using OSaft.applications;
using OSaft.filesystem;
using OSaft.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;

namespace OSaft.usersystem
{
    public static class UserHelper
    {
        internal readonly static Dictionary<string, User> users = new();
        private static string currentUser = "";
        public static string CurrentUser
        {
            get => currentUser;
            internal set => currentUser = value;
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
            ColorTheme.ApplyTheme();
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
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                password = Console.ReadLine();
                ColorTheme.ApplyTheme();
            } while (password == null);
            Console.WriteLine("Getting ready. This may take a moment. May you get a cup of coffee.");

            User rootUser = new(name, password, null, true, 0, false);


            if (!Directory.Exists("system"))
                FileSystem.CreateDirectory("system");

            if (!Directory.Exists("users"))
                FileSystem.CreateDirectory("users");
            FileSystem.CreateDirectory(@"users\" + name);
            FileSystem.CreateFile(@"system\root_init.sys");
            //StartupScreen.Toggle();
            UserDatabase.SaveAllUsers(new List<User> { rootUser });

            ColorTheme.ChangeColor();
            Console.WriteLine("\nSudo user created successfully.");
            Console.WriteLine("--------------------------------");

            // Nach erfolgreicher Einrichtung direkt zum Login
            LoginUser();
        }

        public static void LoginUser()
        {
            List<User> users = UserDatabase.LoadAllUsers();
            if (users.Count == 0)
            {
                ColorTheme.ChangeColor();
                Console.WriteLine("No users found. Please reset filesystem (Skript on Discord)(to be changed).");
                return;
            }

            string name = "";

            for (; ; )
            {
                while (string.IsNullOrWhiteSpace(name))
                {
                    //Console.WriteLine("Skip Login with 'dev'");
                    Console.Write("Enter user name: ");
                    name = Console.ReadLine();
                    // if (name == "dev") { return; }
                }
                User found = users.Find(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                ColorTheme.ChangeColor();
                if (found == null)
                {
                    Console.WriteLine("User not found.");
                    LoginUser();
                    return;
                }
                Console.Write("Enter password: ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                string password = Console.ReadLine();
                ColorTheme.ChangeColor();

                if ((found.PasswordHash == User.ComputeFNV1a(password + found.Salt)))
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
        private static bool dbcheck = false;
        private static bool dblocked = false;

        public static void DBInit()
        {
            if (!File.Exists(@"0:\system\users.db"))
            {

                FileSystem.CreateFile(@"system\users.db");

            }

        }

        public static void SaveAllUsers(List<User> users)
        {
            DBInit();

            string sb = "";
            foreach (User u in users)
            {
                sb += (u.Name + '|' + u.PasswordHash + '|' + u.Salt + '|' + (u.IsSuperuser ? '1' : '0') + '|' + u.Uid.ToString() + '\n');
            }
            if (dblocked == true)
            {
                Console.WriteLine("--------------------------------");
                Console.WriteLine("[UserDB] ERROR: Changes rejected! Changes are currently blocked.");
                Console.WriteLine("--------------------------------");
                dbcheck = false;
                return;
            }
            FileSystem.WriteFile(user_db_path, sb);
            FileSystem.WriteFile(@"\system\root_init.sys", OSaftSecurity.FNV1a(sb));
            dbcheck = false;
            ColorTheme.ChangeColor();
            Console.WriteLine("[UserDB] Saved " + users.Count + " user(s)");
        }

        public static List<User> LoadAllUsers()
        {
            List<User> list = new();

            string content = FileSystem.ReadFile(user_db_path);
            if (string.IsNullOrWhiteSpace(content))
            {
                Console.WriteLine("[UserDB] No content found in users.db");
                return list;
            }
            if (dbcheck == false)
            {
                string hashCheck = FileSystem.ReadFile(@"\system\root_init.sys");
                if (OSaftSecurity.FNV1a(content) != hashCheck)
                {
                    Console.WriteLine("--------------------------------");
                    Console.WriteLine("[UserDB] Warning: users.db integrity check failed! Files could be corrupted!");
                    Console.WriteLine("No recovery available. You are on yourself. Good Luck!");
                    Console.WriteLine("--------------------------------");
                    dbcheck = true;
                    dblocked = true;
                } else { dblocked = false; }
            }
            string[] lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] parts = line.Split('|');
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

            //Console.WriteLine("[UserDB] Loaded " + list.Count + " user(s)");
            return list;
        }

        public static bool GetUserState(string name)
        {
            List<User> users = LoadAllUsers();
            User found = users.Find(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (found != null)
            {
                return found.IsSuperuser;
            }
            else
            {
                throw new KeyNotFoundException("User not found: " + name);
            }
        }
        public static uint GetUserID(string name)
        {
            List<User> users = LoadAllUsers();
            User found = users.Find(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (found != null)
            {
                return found.Uid;
            }
            else
            {
                throw new KeyNotFoundException("User not found: " + name);
            }
            
        }
        public static void ListUsers()
        {
            List<User> users = LoadAllUsers();
            if (users.Count == 0)
            {
                Console.WriteLine("No users found.");
                return;
            }
            else
            {
                Console.WriteLine("[UserDB] Loaded " + users.Count + " user(s)");
                foreach (User u in users)
                {
                    Console.WriteLine($"Name: {u.Name}, UID: {u.Uid}");
                }
            }
        }
        public static void AddUser(string name)
        {
            List<User> users = LoadAllUsers();
            if (users.Exists(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("User already exists: " + name);
                return;
            }
            string password;
            do
            {
                Console.Write("Enter password for " + name + ": ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                password = Console.ReadLine();
                ColorTheme.ChangeColor();
            } while (password == null);

            bool sudo = false;
            string tmp = null;
            while (tmp == null)
            {
                Console.Write("Should the user have sudo privileges? (y/n): ");
                tmp = Console.ReadLine();
                if (tmp.ToLower() == "y")
                {
                    sudo = true;
                }
                else if (tmp.ToLower() == "n")
                {
                    sudo = false;
                }
                else
                {
                    Console.WriteLine("Invalid input. Please enter 'y' or 'n'.");
                    tmp = null;
                    Console.WriteLine();
                }
            }
            int nextUid = 0;
            while (users.Exists(u => u.Uid == (uint)nextUid))
            {
                nextUid++;
            }


            User newUser = new(name, password, null, sudo, (uint)nextUid, false);
            users.Add(newUser);
            FileSystem.CreateDirectory($@"users\{name}");
            SaveAllUsers(users);
            if (dblocked == true) { return; }
            Console.WriteLine("User added successfully: " + name);

        }
        public static void RemoveUser(string name)
        {
            if (name == UserHelper.CurrentUser)
            {
                Console.WriteLine("You can't delete yourself!");
                return;
            }
            List<User> users = LoadAllUsers();
            User found = users.Find(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (found == null)
            {
                Console.WriteLine("User not found: " + name);
                return;
            }

            if (found.Uid == 0)
            {
                Console.WriteLine("You can't kill the gardener!");
                return;
            }
            users.Remove(found);
            SaveAllUsers(users);
            if (dblocked == true) { return; }
            if (FileSystem.DeleteDirectory($@"users\{name}"))
            {
                Console.WriteLine("User removed successfully: " + name);
            }
            else
            {
                Console.WriteLine("Couldn't removed User " + name + ". ");
            }
        }
        public static bool VerifyUser(string name)
        {
            List<User> users = LoadAllUsers();

            User found;
            try
            {
                found = users.Find(u => u.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                Console.WriteLine("User not found: " + name);
                return false;
            }
            Console.WriteLine("Verifying user: " + name);

            Console.Write("Enter password for user " + name + ": ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            string password = Console.ReadLine();
            ColorTheme.ChangeColor();

            if ((found.PasswordHash == User.ComputeFNV1a(password + found.Salt)))
            {
                ColorTheme.ChangeColor();
                Console.WriteLine("User verified successfully: " + name);
                Console.WriteLine("--------------------------------");
                return true;
            }
            else
            {
                Console.WriteLine("Incorrect password. Access denied for user: " + name);
                Console.WriteLine("--------------------------------");
                return false;
            }
        }

        public static void Chdbin()
        {
            string content = FileSystem.ReadFile(user_db_path);
            string hashCheck = FileSystem.ReadFile(@"\system\root_init.sys");
            if (OSaftSecurity.FNV1a(content) != hashCheck)
            {
                Console.WriteLine("--------------------------------");
                Console.WriteLine("[UserDB] Warning: users.db integrity check failed! Files could be corrupted!");
                Console.WriteLine("No recovery available. You are on yourself. Good Luck!");
                Console.WriteLine("--------------------------------");
                dbcheck = true;
                dblocked = true;
            }
            else 
            { 
             dblocked = false;
                Console.WriteLine("--------------------------------");
                Console.WriteLine("[UserDB] Integrity check passed.");
                Console.WriteLine("--------------------------------");
            }
        }
    }
}
