using System;
using System.IO;
using OSaft.filesystem;

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
                Console.WriteLine(command.Name + " - {description}");
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

    // @todo Parameterliste
    public class LSCommand : Command
    {
        public LSCommand(string name) : base(name) { }
        internal override void Execute(string[] args)
        {
            string path = (args == null) ? "" : args[0];
            string[] directories = Directory.GetDirectories(@"0:\" + path);
            string[] files = Directory.GetFiles(@"0:\" + path);
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
        internal override void Execute(string[] args)
        {
            string flag = null;
            string path = null;

            if (args == null || args.Length == 0)
            {
                Console.WriteLine("rm: Syntax: rm < -f|-d > {PATH}");
                return;
            }
            else if (args.Length == 1)
            {
                flag = args[0];
            }
            else if (args.Length == 2)
            {
                flag = args[0];
                path = args[1];
            } 


            if (flag != null && path != null)
            {
                if (flag == "-d")
                {
                    FileSystem.DeleteDirectory(path);
                    //siehe #22: directory, path and file validation notwendig für path in args[1]test
                }

                else if (flag == "-f")
                {
                    FileSystem.DeleteFile(path);
                    //siehe #22: directory, path and file validation notwendig für path in args[1]
                }
            }

            else if (flag == "--help" || flag == "-h")
            {
                Console.WriteLine("HELP-CENTER");
                Console.WriteLine("----------------");
                Console.WriteLine("rm: Remove a file or directory.");
                Console.WriteLine("Syntax: rm < -f|-d > {PATH}");
            }

            else
            {
                Console.WriteLine("rm: Syntax: rm < -f|-d > {PATH}");
            }

        }
    }
}