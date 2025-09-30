using System;
using System.Collections.Generic;
using System.Text;
using Sys = Cosmos.System;

namespace CosmosKernel1
{
    public class Kernel : Sys.Kernel
    {
        readonly Sys.FileSystem.CosmosVFS fs = new();
        protected override void BeforeRun()
        {
            Sys.FileSystem.VFS.VFSManager.RegisterVFS(fs);
            Console.WriteLine("Cosmos booted successfully. Type a line of text to get it echoed back.");
        }

        protected override void Run()
        {
            // Input Handling
            Console.Write("Input: ");
            String input = Console.ReadLine();

            switch (input)
            {
                case "shutdown":
                    Cosmos.System.Power.Shutdown();
                    break;
            }


            // Get Available Space
            long availableSpace = fs.GetAvailableFreeSpace(@"0:\");
            Console.WriteLine("Available Space: " + availableSpace);

        }
    }
}
