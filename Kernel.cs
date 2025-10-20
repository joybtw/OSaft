using Cosmos.System.ScanMaps;
using OSaft.applications;
using OSaft.filesystem;
using OSaft.usersystem;
using System;
using Commands = OSaft.commands;
using Sys = Cosmos.System;

namespace OSaft
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
            Commands.CommandHelper.RegisterCommand(new Commands.CDCommand("cd"));
            Commands.CommandHelper.RegisterCommand(new Commands.ClearCommand("clear"));
            Commands.CommandHelper.RegisterCommand(new Commands.TimeCommand("clock"));
            Commands.CommandHelper.RegisterCommand(new Commands.FileEditCommand("edit"));
            Commands.CommandHelper.RegisterCommand(new Commands.FindCommand("find"));
            Commands.CommandHelper.RegisterCommand(new Commands.FSInfoCommand("fsinfo"));
            Commands.CommandHelper.RegisterCommand(new Commands.HelpCommand("help"));
            Commands.CommandHelper.RegisterCommand(new Commands.LSCommand("ls"));
            Commands.CommandHelper.RegisterCommand(new Commands.MKDirCommand("mkdir"));
            Commands.CommandHelper.RegisterCommand(new Commands.RelogCommand("logout"));
            Commands.CommandHelper.RegisterCommand(new Commands.RMCommand("rm"));
            Commands.CommandHelper.RegisterCommand(new Commands.RebootCommand("reboot"));
            Commands.CommandHelper.RegisterCommand(new Commands.ShutdownCommand("shutdown"));
            Commands.CommandHelper.RegisterCommand(new Commands.TouchCommand("touch"));
            Commands.CommandHelper.RegisterCommand(new Commands.UserCommand("user"));
            Commands.CommandHelper.RegisterCommand(new Commands.VersionCommand("version"));


            // Usersystem
            UserHelper.Initialize();

            // FileSystem Init
            FileSystem.CurrentPath = "0:\\";
        }

        protected override void Run()
        {
            // Input Handling
            Console.Write(UserHelper.CurrentUser + "@" + FileSystem.CurrentPath + "> ");
            String input = Console.ReadLine();
            Commands.CommandHelper.ExecuteCommand(input);
        }
    }
}
