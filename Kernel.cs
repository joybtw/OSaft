using Cosmos.System.ScanMaps;
using OSaft.fileeditor;
using OSaft.filesystem;
using OSaft.usersystem;
using System;
using Commands = OSaft.commands;
using Sys = Cosmos.System;

namespace OSaftKernel
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            SetKeyboardScanMap(new DE_Standard());

            // Welcome Messages
            Console.Clear();    
            Console.WriteLine("OSaft - OperatingSystem - All For Techies");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("OSaft booted correctly");

            // Filesystem
            Sys.FileSystem.VFS.VFSManager.RegisterVFS(FileSystem.Instance);

            // Commands Registry 
            Commands.CommandHelper.RegisterCommand(new Commands.HelpCommand("help"));
            Commands.CommandHelper.RegisterCommand(new Commands.ShutdownCommand("shutdown"));
            Commands.CommandHelper.RegisterCommand(new Commands.VersionCommand("version"));
            Commands.CommandHelper.RegisterCommand(new Commands.RebootCommand("reboot"));
            Commands.CommandHelper.RegisterCommand(new Commands.FSInfoCommand("fsinfo"));
            Commands.CommandHelper.RegisterCommand(new Commands.LSCommand("ls"));
            Commands.CommandHelper.RegisterCommand(new Commands.TouchCommand("touch"));
            Commands.CommandHelper.RegisterCommand(new Commands.MKDirCommand("mkdir"));
            Commands.CommandHelper.RegisterCommand(new Commands.RMCommand("rm"));
            Commands.CommandHelper.RegisterCommand(new Commands.FindCommand("find"));
            Commands.CommandHelper.RegisterCommand(new Commands.FileEditCommand("edit"));
            Commands.CommandHelper.RegisterCommand(new Commands.ClearCommand("clear"));
            Commands.CommandHelper.RegisterCommand(new Commands.CDCommand("cd"));

            // Usersystem
            UserHelper.Initialize();
            //root_user creation working
            //login in progress
            //user safe missing
            FileSystem.CurrentPath = "0:\\";
            //Console.WriteLine(FileSystem.ValidatePath("../", FileSystem.CurrentPath));
            //Console.WriteLine(FileSystem.ValidatePath("0:\\test\\1234", FileSystem.CurrentPath));
            //Console.WriteLine(FileSystem.ValidatePath("../TEST", FileSystem.CurrentPath));
            //Console.WriteLine(FileSystem.ValidatePath("0:\\", FileSystem.CurrentPath));
            //Console.WriteLine(FileSystem.ValidatePath("0:/", FileSystem.CurrentPath));
        }

        protected override void Run()
        {
            // Input Handling
            Console.Write(FileSystem.CurrentPath + "> ");
            String input = Console.ReadLine();
            Commands.CommandHelper.ExecuteCommand(input);
        }
    }
}
