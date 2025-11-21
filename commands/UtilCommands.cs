using OSaft.applications;
using OSaft.filesystem;
using OSaft.usersystem;
using System;
using System.IO;
using OSaft.utils;
using Cosmos.HAL.BlockDevice.Registers;

namespace OSaft.commands
{
    public class HelpCommand : Command
    {
        public HelpCommand(string name) : base(name, "Displays all Commands.")
        {
            this.ParameterList = new();
        }
        internal override void Execute(string[] args)
        {
            Console.WriteLine("Alle Befehle: ");
            foreach (Command command in CommandHelper.Commands.Values)
            {
                Console.WriteLine(command.Name + " - " + command.Description);
            }
        }
    }

    public class ShutdownCommand : Command
    {
        public ShutdownCommand(string name) : base(name, "Shutdowns the system.") { }

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
        public FileEditCommand(string name) : base(name, "Opens the fileeditor for a existing file specified.")
        {
            this.ParameterList.Insert(0, new CommandParameter("path", "Path to editable fale", true, false, val =>
            {
                
                 if (!PermissionManager.CanWrite(FileSystem.ValidatePath(FileSystem.CurrentPath + (val ?? ""), FileSystem.CurrentPath), UserDatabase.GetUserID(UserHelper.CurrentUser)))
                 {
                     Console.WriteLine("Error: No permission to rewrite this file.");
                     return;
                 }
                 if (!PermissionManager.CanRead(FileSystem.ValidatePath(FileSystem.CurrentPath + (val ?? ""), FileSystem.CurrentPath), UserDatabase.GetUserID(UserHelper.CurrentUser)))
                 {
                     Console.WriteLine("Error: No permission to read this file.");
                     return;
                 }
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
        public LSCommand(string name) : base(name, "List all directories and files in a specific (or current) path")
        {
            this.ParameterList.Insert(0, new CommandParameter("path", "Path to list", false, false, path =>
            {
                string temp = FileSystem.ValidatePath(path, FileSystem.CurrentPath);
                if (!Directory.Exists(temp)) { Console.WriteLine("Directory does not exist."); return; }
                if (!PermissionManager.CanRead(temp, UserDatabase.GetUserID(UserHelper.CurrentUser)))
                { Console.WriteLine("Error: No permission to read this directory."); return; }
                LsExecute(path);

            }));
        }

        private static void LsExecute(string path)
        {
 
            string[] directories = Directory.GetDirectories(FileSystem.ValidatePath(FileSystem.CurrentPath + (path ?? ""), FileSystem.CurrentPath));
                string[] files = Directory.GetFiles(FileSystem.ValidatePath(FileSystem.CurrentPath + (path ?? ""), FileSystem.CurrentPath));
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
        public TouchCommand(string name) : base(name, "Creates a new file.")
        {
            this.ParameterList.Insert(0, new CommandParameter("path", "which file should be created", true, false, path =>
            {
                if (!PermissionManager.CanWrite(FileSystem.CurrentPath, UserDatabase.GetUserID(UserHelper.CurrentUser)))
                {
                    Console.WriteLine("Error: No permission to write in this directory.");
                    return;
                }
                PermissionManager.AddRights(FileSystem.ValidatePath(FileSystem.CurrentPath + (path ?? ""), FileSystem.CurrentPath), UserDatabase.GetUserID(UserHelper.CurrentUser));
                FileSystem.CreateFile(path);
            }));
        }
    }

    public class MKDirCommand : Command
    {
        public MKDirCommand(string name) : base(name, "Creates a new directory.")
        {
            this.ParameterList.Insert(0, new CommandParameter("directory", "which directory should be created", true, false, path =>
            {
                if (!PermissionManager.CanWrite(FileSystem.CurrentPath, UserDatabase.GetUserID(UserHelper.CurrentUser)))
                {
                    Console.WriteLine("Error: No permission to write in this directory.");
                    return;
                }
            string _path = FileSystem.ValidatePath(FileSystem.CurrentPath + (path ?? ""), FileSystem.CurrentPath);
                PermissionManager.AddRights(_path + "\\", UserDatabase.GetUserID(UserHelper.CurrentUser));
                FileSystem.CreateDirectory(path);
            }));
        }
    }

    public class RMCommand : Command
    {
        public RMCommand(string name) : base(name, "Removes a file (-f) or directory (-d).")
        {
            ParameterList.Add(new CommandParameter("-f", "Delete a file", false, true, val =>
            {
                if (string.IsNullOrEmpty(val))
                {
                    Console.WriteLine(Name + ": Error: no file provided.");
                    return;
                }
                if (!PermissionManager.CanWrite(FileSystem.ValidatePath(FileSystem.CurrentPath + (val ?? ""), FileSystem.CurrentPath), UserDatabase.GetUserID(UserHelper.CurrentUser)))
                {
                    Console.WriteLine("Error: No permission to write in this directory.");
                    return;
                }

                if (FileSystem.DeleteFile(val))
                {
                    Console.WriteLine("File " + val + " was deleted.");
                    PermissionManager.RemoveEntry(FileSystem.ValidatePath(FileSystem.CurrentPath + (val ?? ""), FileSystem.CurrentPath));
                }
            }));

            ParameterList.Add(new CommandParameter("-d", "Delete a directory", false, true, val =>
            {
                if (string.IsNullOrEmpty(val))
                {
                    Console.WriteLine(Name + ": Error: no directory provided.");
                    return;
                }

                if (FileSystem.DeleteDirectory(val)) { Console.WriteLine("Directory " + val + " was deleted.");
                    PermissionManager.RemoveEntry(FileSystem.ValidatePath(FileSystem.CurrentPath + (val ?? ""), FileSystem.CurrentPath));
                }
            }));

            ParameterList.Add(new CommandParameter("--errortest", "Just a test. To be removed.", false, false, val =>
            {
                new Error(Name, "Dies ist ein Test-Error!", Error.criticality.VeryCritical, true).CreateNewLog();
            }));

            
        }
    }

    public class FindCommand : Command
    {
        private static string filename;
        private static string startPath = "";
        public FindCommand(string name) : base(name, "Finds a file with the specified filename.")
        {
            this.ParameterList.Insert(0, new CommandParameter("filename", "Name of file for the search", true, false, val => filename = val));
            this.ParameterList.Insert(1, new CommandParameter("startPath", "Path to start the search (Default: .)", false, false, val => startPath = val));
        }

        internal override void Execute(string[] args)
        {
            ExecuteParams(args);
            Console.WriteLine(string.Join("\n", FileSystem.FindFile(filename, startPath)));
        }
    }

    public class CDCommand : Command
    {
        public CDCommand(string name) : base(name, "Changes the current directory.")
        {

            string temp = FileSystem.CurrentPath;
            this.ParameterList.Insert(0, new CommandParameter("path", "Path to navigate into", true, false, path =>
            {
                if (path == null) { Console.WriteLine("Please provide a path."); return; }
                
                string temp = FileSystem.ValidatePath(path, FileSystem.CurrentPath);
                if (!Directory.Exists(temp)) { Console.WriteLine("Directory does not exist."); return; }
                if (!PermissionManager.CanRead(temp, UserDatabase.GetUserID(UserHelper.CurrentUser)))
                {

                    Console.WriteLine("Error: No permission to read this directory.");
                    return;
                }
                FileSystem.CurrentPath = path;
               
            }));

        }
    }
    public class UserCommand : Command
    {
        public UserCommand(string name) : base(name, "Creates, removes or lists a user.")
        {
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
            }
            else
            {
                Console.WriteLine("Please specify at least one argument.");
            }
        }
    }
    public class RelogCommand : Command
    {
        public RelogCommand(string name) : base(name, "Locks the access.") { }
        internal override void Execute(string[] args)
        {
            if (args.Length == 0) { UserHelper.LoginUser(); FileSystem.CurrentPath = "0:\\"; }
            ExecuteParams(args);
        }
    }

    public class ChangeNameCommand : Command
    {
        public ChangeNameCommand(string name) : base(name, "Changes the name of an user.") { }
        internal override void Execute(string[] args)
        {
            if (args.Length > 0)
            {
                if (!string.IsNullOrWhiteSpace(args[0])) { Console.WriteLine("Please provide a valid username"); return; }
                if (!string.IsNullOrWhiteSpace(args[1])) { Console.WriteLine("New name can't be null or whitespace."); return; }
                UserDatabase.ChangeName(args[0], args[1]);
            }
            else
            {
                Console.WriteLine("Syntax: chname <oldname> <newname>");
                ExecuteParams(args);
            }

        }
    }

    public class ChangePasswordCommand : Command
    {
        public ChangePasswordCommand(string name) : base(name, "Changes the current user's name.")
        { }
        internal override void Execute(string[] args)
        {
            if (args.Length == 0)
            { UserDatabase.ChangePw(); }
            ExecuteParams(args);
        }
    }


    public class SudoCommand : Command
    {
        public SudoCommand(string name) : base(name, "Changes the user's sudo status.")
        {
            this.ParameterList.Add(new CommandParameter("-a", "Adds the user state", false, true, user => UserDatabase.MakeSudo(user)));
            this.ParameterList.Add(new CommandParameter("-r", "Removes the user state", false, true, user => UserDatabase.DeSudo(user)));

        }
        internal override void Execute(string[] args)
        {
            if (args.Length > 0)
            {
                ExecuteParams(args);
            }
            else
            {
                Console.WriteLine("Please specify at least one argument.");
            }
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

    public class CheckDBIntegrityCommand : Command
    {
        public CheckDBIntegrityCommand(string name) : base(name, "Checks the integrity of the user database.\n                           Unblocks/Blocks changes to DB depending on current\n                           integrity state.") { }
        internal override void Execute(string[] args)
        {
            if (args.Length == 0) { UserDatabase.Chdbin(); }
            ExecuteParams(args);
        }
    }
    
    public class ChangePermissionsCommand : Command
    {
        private string permString;
        private string path;
        private string user;
        public ChangePermissionsCommand(string name) : base(name, "Creates, removes or lists a permissions.")
        {
            // CommandParameter(string name, string description, bool isRequired, bool expectsParameters, Action<string> action)
            this.ParameterList.Insert(0, new CommandParameter("path", "Path to permission", true, false, (val) => path = val));
            this.ParameterList.Insert(1, new CommandParameter("permission", "Removes writing permission to user", true, false, (val) => permString = val));
            this.ParameterList.Insert(2, new CommandParameter("user", "User for permission", true, false, (val) => user = val));
        }

        private void HandlePermissions()
        {
            // user, path, permString available for edit

            path = FileSystem.ValidatePath(path, FileSystem.CurrentPath);

            switch (permString)
            {

                case ("+r"):
                    if (!UserDatabase.GetUserState(UserHelper.CurrentUser)) { Console.WriteLine("No permission"); return; }
                    if (!UserDatabase.VerifyUser(UserHelper.CurrentUser)) { return; }
                    PermissionManager.ShouldRead(path, user);
                    break;
                case ("+w"):
                    if (!UserDatabase.GetUserState(UserHelper.CurrentUser)) { Console.WriteLine("No permission"); return; }
                    if (!UserDatabase.VerifyUser(UserHelper.CurrentUser)) { return; }
                    PermissionManager.ShouldWrite(path, user);
                    break;
                case ("+rw"):
                    if (!UserDatabase.GetUserState(UserHelper.CurrentUser)) { Console.WriteLine("No permission"); return; }
                    if (!UserDatabase.VerifyUser(UserHelper.CurrentUser)) { return; }
                    PermissionManager.ShouldBoth(path, user);
                    break;
                case ("-r"):
                    if (!UserDatabase.GetUserState(UserHelper.CurrentUser)) { Console.WriteLine("No permission"); return; }
                    if (!UserDatabase.VerifyUser(UserHelper.CurrentUser)) { return; }
                    PermissionManager.ShouldntRead(path, user);
                    break;
                case ("-w"):
                    if (!UserDatabase.GetUserState(UserHelper.CurrentUser)) { Console.WriteLine("No permission"); return; }
                    if (!UserDatabase.VerifyUser(UserHelper.CurrentUser)) { return; }
                    PermissionManager.ShouldntWrite(path, user);
                    break;
                case ("-rw"):
                    if (!UserDatabase.GetUserState(UserHelper.CurrentUser)) { Console.WriteLine("No permission"); return; }
                    if (!UserDatabase.VerifyUser(UserHelper.CurrentUser)) { return; }
                    PermissionManager.ShouldntBoth(path, user);
                    break;


                default:
                    throw new Exception("Error: invalid permstring try: +r, +w, +rw, -r, -w or -rw  = ");

            }
        }

        internal override void Execute(string[] args)
        {
            if (args.Length > 0)
            {
                ExecuteParams(args);

                HandlePermissions();
            }
   
        }
}
   
    public class ShowRightsCommand : Command
    {
        public ShowRightsCommand(string name) : base(name, "Displays Owner of a directory and the permissions of the current user for it. ") { 
        
        }
        internal override void Execute(string[] args)
        {
            string path = "";
            if (args.Length == 0) { path = FileSystem.CurrentPath; }
            else
            {
                path = args[0];
                path = FileSystem.ValidatePath(path, FileSystem.CurrentPath);

            }
            PermissionManager.Display(path);
            ExecuteParams(args);
        
        }
    }

   public class ChangeOwnerCommand : Command
        {
        private string path;
        private string user;
        public ChangeOwnerCommand(string name) : base(name, "Exchanges the owner of a file or directory") 
        {
            this.ParameterList.Insert(0, new CommandParameter("path", "Path to permission", true, false, (val) => path = val));
            this.ParameterList.Insert(1, new CommandParameter("user", "User for permission", true, false, (val) => user = val));
        }

        internal override void Execute(string[] args)
        {
            if (args.Length > 0) { 
                PermissionManager.ChangeOwner(path, user);
                ExecuteParams(args);
            }
            
        }


    }
 


}

// Gabriel was here <3