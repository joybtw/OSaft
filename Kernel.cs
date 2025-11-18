using Cosmos.System.ScanMaps;
using OSaft.applications;
using OSaft.filesystem;
using OSaft.usersystem;
using OSaft.utils;
using System;
using Commands = OSaft.commands;
using Sys = Cosmos.System;

namespace OSaft
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            // Filesystem
            Sys.FileSystem.VFS.VFSManager.RegisterVFS(FileSystem.Instance);

            ColorTheme.ApplyTheme();   
            SetKeyboardScanMap(new DE_Standard());

            StartupScreen.Init();

            ColorTheme.ApplyTheme();
            Console.SetCursorPosition(0, 0);

            // Welcome Messages
            Console.WriteLine("OSaft - OperatingSystem - All for Techies");
            Console.WriteLine("--------------------------------");


            // Commands Registry 
            Commands.CommandHelper.RegisterCommand(new Commands.CheckDBIntegrityCommand("check_database_integrity"));
            Commands.CommandHelper.RegisterCommand(new Commands.CDCommand("cd"));
            Commands.CommandHelper.RegisterCommand(new Commands.ClearCommand("clear"));
            Commands.CommandHelper.RegisterCommand(new Commands.ClockCommand("clock"));
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
            Commands.CommandHelper.RegisterCommand(new Commands.ToggleStartScreenCommand("toggle_startscreen"));
            Commands.CommandHelper.RegisterCommand(new Commands.ToggleClockCommand("toggle_clock"));
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
            ColorTheme.ChangeColor();
            string input = Shell.ReadLineWithHistory(Clock.GetShortTime() + UserHelper.CurrentUser + "@" + FileSystem.CurrentPath + "> ");
            Commands.CommandHelper.ExecuteCommand(input);
        }
    }
}
