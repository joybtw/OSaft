using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;
using Commands = OSaft.commands;

namespace CosmosKernel1
{
    public class Kernel : Sys.Kernel
    {
        readonly Sys.FileSystem.CosmosVFS fs = new();
        protected override void BeforeRun()
        {
            Sys.FileSystem.VFS.VFSManager.RegisterVFS(fs);
            Console.WriteLine("OSaft - OperatingSystem - All For Techies");
            Console.WriteLine("--------------------------------");
            Console.WriteLine("OSaft booted correctly");
            long availableSpace = fs.GetAvailableFreeSpace(@"0:\");
            Console.WriteLine("Available Space: " + availableSpace);
            Commands.CommandHelper.RegisterCommand(new Commands.HelpCommand("help"));
            Commands.CommandHelper.RegisterCommand(new Commands.ShutdownCommand("shutdown"));
            Commands.CommandHelper.RegisterCommand(new Commands.VersionCommand("version"));
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
