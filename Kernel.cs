using Cosmos.System.ScanMaps;
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
            // Welcome Messages
            Console.WriteLine("OSaft - OperatingSystem - All For Techies");
            SetKeyboardScanMap(new DE_Standard());
            Console.WriteLine("--------------------------------");
            Console.WriteLine("OSaft booted correctly");

            // FileSystem
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

            // User System
            Init_User.Initialize();
            //root_user creation working
            //login in progress
            //user safe missing
        }

        protected override void Run()
        {
            // Input Handling
            Console.Write("Input: ");
            String input = Console.ReadLine();
            Commands.CommandHelper.ExecuteCommand(input);
        }
    }
}
