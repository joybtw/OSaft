using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using Commands = OSaft.commands;

namespace OSaftKernel
{
    public class Kernel : Sys.Kernel
    {
        public Sys.FileSystem.CosmosVFS FS { get; } = new();
        public static Kernel Instance { get; private set; }

        public Kernel() {
            Instance = this;
        }

        protected override void BeforeRun()
        {

            Sys.FileSystem.VFS.VFSManager.RegisterVFS(FS);
            Console.WriteLine("OSaft - OperatingSystem - All For Techies");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("OSaft booted correctly");
            Commands.CommandHelper.RegisterCommand(new Commands.HelpCommand("help"));
            Commands.CommandHelper.RegisterCommand(new Commands.ShutdownCommand("shutdown"));
            Commands.CommandHelper.RegisterCommand(new Commands.VersionCommand("version"));
            Commands.CommandHelper.RegisterCommand(new Commands.RebootCommand("reboot"));
            Commands.CommandHelper.RegisterCommand(new Commands.DiskSpaceCommand("diskspace"));
        }

        protected override void Run()
        {
            // Input Handling
            Console.Write("Input: ");
            String input = Console.ReadLine();
            Commands.CommandHelper.ExecuteCommand(input, input.Split(" "));
        }
    }
}
