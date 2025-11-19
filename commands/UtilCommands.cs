using OSaft.applications;
using OSaft.filesystem;
using OSaft.usersystem;
using System;
using System.IO;
using OSaft.utils;
using OSaft.filesystem;

namespace OSaft.commands
{
    public class HelpCommand : Command
    {
        public HelpCommand(string name) : base(name, "Displays all commands.")
        {
            this.ParameterList = new();
        }
        internal override void Execute(string[] args)
        {
            Console.WriteLine("Alle Befehle: ");
            foreach (Command command in CommandHelper.commands.Values)
            {
                Console.WriteLine(command.Name + " - " + command.Description);
            }
        }
    }

    public class ShutdownCommand : Command
    {
        public ShutdownCommand(string name) : base(name, "Shutdowns the system.") {}

        internal override void Execute(string[] args)
        {
            ExecuteParams(args);
            if (args.Length == 0)
            {
                Console.WriteLine("Shutdown in 5s. Goodbye!");
                System.Threading.Thread.Sleep(5000);
                Cosmos.System.Power.Shutdown();
            }
        }
    }
    public class VersionCommand : Command
    {
        public VersionCommand(string name) : base(name, "Version of the operating system.") { }

        internal override void Execute(string[] args)
        {
            ExecuteParams(args);
            if (args.Length == 0) { Console.WriteLine("OSaft v0.1 - early alpha"); }
        }
    }

    public class RebootCommand : Command
    {
        public RebootCommand(string name) : base(name, "Reboots the system.") { }

        internal override void Execute(string[] args)
        {
            ExecuteParams(args);
            if (args.Length == 0)
            {
                Console.WriteLine("Goodbye. See you soon!");
                Cosmos.System.Power.Reboot();
            }
        }
    }

    public class FSInfoCommand : Command
    {
        public FSInfoCommand(string name) : base(name, "Gets important infos for the filesystem.") { }
        internal override void Execute(string[] args)
        {
            ExecuteParams(args);
            if (args.Length == 0)
            {
                Console.WriteLine("Available Space: " + FileSystem.Instance.GetAvailableFreeSpace(@"0:\"));
                Console.WriteLine("File System Type: " + FileSystem.Instance.GetFileSystemType(@"0:\"));
            }
        }
    }

    public class FileEditCommand : Command
    {
        public FileEditCommand(string name) : base(name, "Opens the fileeditor for a existing file specified.") {
            this.ParameterList.Insert(0, new CommandParameter("path", "Path to editable fale", true, false, val =>
            {
                FileEditor.EditFile(val);
                ColorTheme.ApplyTheme();
            }));
        }
    }

    public class ClearCommand : Command
    {
        public ClearCommand(string name) : base(name, "Clears the console.") { }

        internal override void Execute(string[] args)
        {
            ExecuteParams(args);
            if (args.Length == 0)
            {
                Console.Clear();
                return;
            }
        }
    }

    public class LSCommand : Command
    {
        public LSCommand(string name) : base(name, "List all directories and files in a specific (or current) path") {
            this.ParameterList.Insert(0, new CommandParameter("path", "Path to list", false, false, path =>
            {
                LsExecute(path);
            }));
        }

        private static void LsExecute(string path) {
            string[] directories = Directory.GetDirectories(FileSystem.CurrentPath + "\\" + path);
            string[] files = Directory.GetFiles(FileSystem.CurrentPath + "\\" + path);
            Console.WriteLine("Directories: " + string.Join(", ", directories));
            Console.WriteLine("Files: " + string.Join(", ", files));
        }
        internal override void Execute(string[] args)
        {
            ExecuteParams(args);
            if (args.Length == 0)
            {
                LsExecute("");
            }
        }
    }

    public class TouchCommand : Command
    {
        public TouchCommand(string name) : base(name, "Creates a new file.") {
            this.ParameterList.Insert(0, new CommandParameter("path", "which file should be created", true, false, path =>
            {
                FileSystem.CreateFile(path);
            }));
        }
    }

    public class MKDirCommand : Command
    {
        public MKDirCommand(string name) : base(name, "Creates a new directory.") {
            this.ParameterList.Insert(0, new CommandParameter("directory", "which directory should be created", true, false, path =>
            {
                FileSystem.CreateDirectory(path);
            }));
        }
    }

    public class RMCommand : Command
    {
        public RMCommand(string name) : base(name, "Removes a file (-f) or directory (-d).") {
            ParameterList.Add(new CommandParameter("-f", "Delete a file", false, true, val => {
                if (string.IsNullOrEmpty(val))
                {
                    Console.WriteLine(Name + ": Error: no file provided.");
                    return;
                }

                if (FileSystem.DeleteFile(val)) { Console.WriteLine("File " + val + " was deleted."); }
            }));

            ParameterList.Add(new CommandParameter("-d", "Delete a directory", false, true, val => {
                if (string.IsNullOrEmpty(val))
                {
                    Console.WriteLine(Name + ": Error: no directory provided.");
                    return;
                }
                if (FileSystem.DeleteDirectory(val)) { Console.WriteLine("Directory " + val + " was deleted."); }
            }));

            ParameterList.Add(new CommandParameter("--errortest", "Just a test. To be removed.", false, false, val =>
            {
                new Error(Name, "Dies ist ein Test-Error!", Error.criticality.Info, true).CreateNewLog();
            }));
        }
    }

    public class FindCommand : Command
    {
        private static string filename;
        private static string startPath = "";
        public FindCommand(string name) : base(name, "Finds a file with the specified filename.") {
            this.ParameterList.Insert(0, new CommandParameter("filename", "Name of file for the search", true, false, val => filename = val));
            this.ParameterList.Insert(1, new CommandParameter("startPath", "Path to start the search (Default: .)", false, false, val => startPath = val));
        }

        internal override void Execute(string[] args) {
            ExecuteParams(args);
            Console.WriteLine(string.Join("\n", FileSystem.FindFile(filename, startPath)));
        }
    }

    public class CDCommand : Command
    {
        public CDCommand(string name) : base(name, "Changes the current directory.") {
            this.ParameterList.Insert(0, new CommandParameter("path", "Path to navigate into", true, false, path => { FileSystem.CurrentPath = path; }));
        }
    }
    public class UserCommand : Command
    {
        public UserCommand(string name) : base(name, "Creates, removes or lists a user.") {
            this.ParameterList.Add(new CommandParameter("-a", "Creates a user", false, true, user => UserDatabase.AddUser(user)));
            this.ParameterList.Add(new CommandParameter("-r", "Removes a user", false, true, user => UserDatabase.RemoveUser(user)));
            this.ParameterList.Add(new CommandParameter("-l", "Lists all users", false, false, _ => UserDatabase.ListUsers()));
            this.ParameterList.Add(new CommandParameter("--add", "Creates a user", false, true, user => UserDatabase.AddUser(user)));
            this.ParameterList.Add(new CommandParameter("--remove", "Removes a user", false, true, user => UserDatabase.RemoveUser(user)));
            this.ParameterList.Add(new CommandParameter("--list", "Lists all users", false, false, _ => UserDatabase.ListUsers()));
        }
        internal override void Execute(string[] args)
        {
            ColorTheme.ChangeColor();
            if (args.Length > 0)
            {
                if (UserDatabase.GetUserState(UserHelper.CurrentUser) != true)
                {
                    Console.WriteLine("Error: No permission!");
                    return;
                }
                if (UserDatabase.VerifyUser(UserHelper.CurrentUser) == false) { return; }
                ExecuteParams(args);
            } else
            {
                Console.WriteLine("Please specify at least one argument.");
            }
        }
    }
    public class RelogCommand : Command
    {
        public RelogCommand(string name) : base(name, "Locks the access.") {}
        internal override void Execute(string[] args) {
            if (args.Length == 0) { UserHelper.LoginUser(); }
            ExecuteParams(args);
        }
    }
    public class ClockCommand : Command
    {
        public ClockCommand(string name) : base(name, "Shows the current time.") { }
        internal override void Execute(string[] args)
        {
            if (args.Length == 0) { Console.WriteLine(Clock.ShowTime()); }
            ExecuteParams(args);
        }
     }
    public class ToggleClockCommand : Command
    {
        public ToggleClockCommand(string name) : base(name, "Toggles the clock.") { }
        internal override void Execute(string[] args)
        {
            if (args.Length == 0) { Clock.ToggleClock(); }
            ExecuteParams(args);
        }
    }

    public class ToggleStartScreenCommand : Command
    {
        public ToggleStartScreenCommand(string name) : base(name, "Toggles the start screen.") { }
        internal override void Execute(string[] args)
        {
            if (args.Length == 0) { StartupScreen.Toggle(); }
            ExecuteParams(args);
        }
    }
}