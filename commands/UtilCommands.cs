using System;

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
            Console.WriteLine("Goodbye.");
            Cosmos.System.Power.Shutdown();
        }
    }
}
