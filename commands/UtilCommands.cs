using System;
using Sys = Cosmos.System;

namespace OSaft.commands
{
    public class HelpCommand: Command
    {
        public HelpCommand(string name): base(name) {}

        internal override void Execute(string[] args) {
            Console.WriteLine("Alle Befehle: ");
            foreach (Command command in CommandHelper.commands.Values)
            {
                Console.WriteLine(command.name + " - {description}");
            }
        }
    }

    public class ShutdownCommand: Command
    {
        public ShutdownCommand(string name): base(name) {}

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


}
