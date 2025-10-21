using Cosmos.System.Graphics.Fonts;
using OSaft.applications;
using OSaft.filesystem;
using OSaft.usersystem;
using System;
using System.IO;
using static Cosmos.HAL.BlockDevice.ATA_PIO;

namespace OSaft.commands
{
    public class HelpCommand : Command
    {
        public HelpCommand(string name) : base(name) { }

        internal override void Execute(string[] args)
        {
            Console.WriteLine("Alle Befehle: ");
            foreach (Command command in CommandHelper.commands.Values)
            {
                //Console.WriteLine(command.Name + " - " + Description);
            }
        }
    }

    public class ShutdownCommand : Command
    {
        public ShutdownCommand(string name) : base(name) { }

        internal override void Execute(string[] args)
        {
            Console.WriteLine("Shutdown in 5s. Goodbye!");
            System.Threading.Thread.Sleep(5000);
            Cosmos.System.Power.Shutdown();
        }
    }
    public class VersionCommand : Command
    {
        public VersionCommand(string name) : base(name) { }

        internal override void Execute(string[] args)
        {
            Console.WriteLine("OSaft v0.1 - early alpha");
        }
    }

    public class RebootCommand : Command
    {
        public RebootCommand(string name) : base(name) { }

        internal override void Execute(string[] args)
        {
            Console.WriteLine("Goodbye. See you soon!");
            Cosmos.System.Power.Reboot();
        }
    }

    public class FSInfoCommand : Command
    {
        public FSInfoCommand(string name) : base(name) { }
        internal override void Execute(string[] args)
        {
            Console.WriteLine("Available Space: " + FileSystem.Instance.GetAvailableFreeSpace(@"0:\"));
            Console.WriteLine("File System Type: " + FileSystem.Instance.GetFileSystemType(@"0:\"));
        }
    }

    public class FileEditCommand : Command
    {
        public FileEditCommand(string name) : base(name) { }

        internal override void Execute(string[] args)
        {
            FileEditor.EditFile(args[0]);
        }
    }

    public class ClearCommand : Command
    {
        public ClearCommand(string name) : base(name) { }

        internal override void Execute(string[] args)
        {
            Console.Clear();
            return;
        }
    }

    // @todo Parameterliste
    public class LSCommand : Command
    {
        public LSCommand(string name) : base(name) { }
        internal override void Execute(string[] args)
        {
            string path = (args == null) ? "" : args[0];
            string[] directories = Directory.GetDirectories(FileSystem.CurrentPath + "\\" + path);
            string[] files = Directory.GetFiles(FileSystem.CurrentPath + "\\" + path);
            Console.WriteLine("Directories: " + string.Join(", ", directories));
            Console.WriteLine("Files: " + string.Join(", ", files));
        }
    }

    public class TouchCommand : Command
    {
        public TouchCommand(string name) : base(name) { }
        internal override void Execute(string[] args)
        {
            if (args.Length == 1)
            {
                FileSystem.CreateFile(args[0]);
            }

            else
            {
                Console.WriteLine("Please specify a filename");
            }
        }
    }

    public class MKDirCommand : Command
    {
        public MKDirCommand(string name) : base(name) { }
        internal override void Execute(string[] args)
        {
            if (args.Length == 1)
            {
                FileSystem.CreateDirectory(args[0]);
            }
            else
            {
                Console.WriteLine("Please specify a Foldername");
            }
        }
    }

    public class RMCommand : Command
    {
        public RMCommand(string name) : base(name) { }
        //internal override void Execute(string[] args)
        //{
        //    //FileSystem.DeleteFile(path);
        //    //FileSystem.DeleteDirectory(path);

            
        //}
        protected override void DefineParameters()
        {
            ParameterList.Add(new CommandParameter("-h", "Displays help", false, false, val => {
                Console.WriteLine(Name + ": deletes a file or directory");
                Console.WriteLine("Syntax: " + GetSyntax());
            }));

            ParameterList.Add(new CommandParameter("--help", "Displays help", false, false, val => {
                Console.WriteLine(Name + ": deletes a file or directory");
                Console.WriteLine("Syntax: " + GetSyntax());
            }));

            ParameterList.Add(new CommandParameter("-f", "Delete a file", false, true, val => {
                if (string.IsNullOrEmpty(val))
                    {
                        Console.WriteLine(Name + ": Error: no file provided.");
                        return;
                    }
                FileSystem.DeleteFile(val);
                Console.WriteLine("File " + val + " was deleted.");
            }));

            ParameterList.Add(new CommandParameter("-d", "Delete a directory", false, true, val => {
                if (string.IsNullOrEmpty(val))
                {
                    Console.WriteLine(Name + ": Error: no directory provided.");
                    return;
                }
                FileSystem.DeleteDirectory(val);
                Console.WriteLine("Directory " + val + " was deleted.");
            }));
        }
    }

    public class FindCommand : Command
    {
        public FindCommand(string name) : base(name) { }

        internal override void Execute(string[] args) {
            if (args.Length > 0) {
                Console.WriteLine(string.Join("\n", FileSystem.FindFile(args[0], args.Length > 1 ? args[1] : null)));
            }
            else
            {
                Console.WriteLine("Error: Syntax: find <filename> <path>");
            }
        }
    }

    public class CDCommand : Command
    {
        public CDCommand(string name) : base(name) { }
        internal override void Execute(string[] args)
        {
            
                if (args.Length ==1 && args != null)
                {
                    FileSystem.CurrentPath = args[0];
                }
                else
                {
                    Console.WriteLine("Error: Syntax: cd <path>");
                }
        }
    }
    public class UserCommand : Command
    {
        public UserCommand(string name) : base(name) { }
        internal override void Execute(string[] args)
        {
            if (UserDatabase.GetUserState(UserHelper.CurrentUser) != true)
            {
                Console.WriteLine("Error: No permission!");
                return;
            }
            if (UserDatabase.VerifyUser(UserHelper.CurrentUser) == false) { return; }

            if (args == null || args.Length < 0)
            {
                Console.WriteLine("Error: Syntax: user <add|remove|list> [username]");
                return;
            }


            switch (args[0].ToLower())
            {
                case "add":
                    if (args.Length != 2)
                    {
                        Console.WriteLine("Error: Syntax: user add <username>");
                        return;
                    }
                    UserDatabase.AddUser(args[1]);
                    break;

                case "remove":
                    if (args.Length != 2)
                    {
                        Console.WriteLine("Error: Syntax: user remove <username>");
                        return;
                    }
                    if (args[1] == UserHelper.CurrentUser)
                    {
                        Console.WriteLine("Error: You can't delete yourself!");
                        return;
                    }
                    UserDatabase.RemoveUser(args[1]);
                    break;

                case "list":
                    UserDatabase.ListUsers();
                    break;

                default:
                    Console.WriteLine("Error: Unknown command " + args[0]);
                    break;
            }
        }
    }
    public class RelogCommand : Command
    {
        public RelogCommand(string name) : base(name) { }
        internal override void Execute(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                Console.WriteLine("Error: Syntax: relog");
                return;
            }
            UserHelper.LoginUser();
        }
    }
    public class TimeCommand : Command
    {
        public TimeCommand(string name) : base(name) { }
        internal override void Execute(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                Console.WriteLine("Error: Syntax: clock");
                return;
            }
            Clock.ShowTime();
        }
     }
    public class LiveTimeCommand : Command
    {
        public LiveTimeCommand(string name) : base(name) { }
        internal override void Execute(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                Console.WriteLine("Error: Syntax: toggle-clock");
                return;
            }
            if (Clock.ActiveClock == true) {Clock.ActiveClock = false; }
            else { Clock.ActiveClock = true; }


        }
    }
}