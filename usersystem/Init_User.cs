using System;
using OSaft.filesystem;
using OSaft.usersystem;
using System.IO;
using Commands = OSaft.commands;
using OSaft.commands;

namespace OSaft.usersystem
{
    public class Init_User
    {
        private string path = @"0:\system\root_init";

        public void Initialize()
        {
            if (File.Exists(path))
            {
                //LoginUser;
                return;
            }

            Console.WriteLine("Welcome to OSaft");
            Console.WriteLine("OSaft needs you to create a user");
        name:
            Console.Write("Enter user name: ");
            String name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("No input or only whitespaces, return!");
                goto name;
            }
        password:
            Console.Write("Enter password: ");
            String password = Console.ReadLine();
            if (password == null)
            {
                Console.WriteLine("No input, return!");
                goto password;
            }

            User root_u = new(name, password, true, 0);
            if (!Directory.Exists("system"))
            {
                FileSystem.CreateDirectory("system", "");
            }
            FileSystem.CreateDirectory("users", "");
            FileSystem.CreateDirectory(name, "users");
            FileSystem.CreateFile("root_init", "system"); 
            Console.WriteLine("sudo user created");
            Console.WriteLine("Your name is: " + name);
            Console.WriteLine("--------------------------------");
            return;
        }
    }

    // work still in progress
    public class Login
    {
        public void LoginUser()
        {
        Login:
            Console.Write("Enter user name: ");
            String name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("No input or only whitespaces, return!");
                //user reg missing
                goto Login;
            }

        
        }
    }
}