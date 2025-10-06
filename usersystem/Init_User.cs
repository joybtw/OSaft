using System;
using OSaft.filesystem;
using System.IO;

namespace OSaft.usersystem
{
    public class Init_User
    {
        public static void Initialize()
        {
            if (File.Exists(@"0:\system\root_init.sys")){ return; }
            Console.WriteLine("Welcome to OSaft");
            Console.WriteLine("OSaft needs you to create a user");


            Console.Write("Enter user name: ");
            String name = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(name))
            {
                Console.Write("Enter user name: ");
                name = Console.ReadLine();
            }

            Console.Write("Enter password: ");
            String password = Console.ReadLine();
            while (string.IsNullOrWhiteSpace(password))
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
            Console.WriteLine("Sudo user created");
            Console.WriteLine("Your name is: " + name);
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
}