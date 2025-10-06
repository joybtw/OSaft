using System;
using System.Collections.Generic;
using System.Linq;

namespace OSaft.commands
{
    public abstract class Command
    {
        public string Name { get; }

        public Command(string name)
        {
            this.Name = name;
        }
        internal abstract void Execute(string[] args);
    }
    public class CommandHelper
    {
        internal readonly static Dictionary<string, Command> commands = new();

        public static void RegisterCommand(Command command)
        {
            if (!commands.ContainsKey(command.Name))
            {
                commands.Add(command.Name, command);
            } else
            {
                Console.WriteLine("Befehl " + command.Name + " existiert bereits.");
            }
        }

        public static void ExecuteCommand(string input)
        {
            try
            {
                if (string.IsNullOrEmpty(input)) { return; }
                string[] args = input.Split(" ");
                //Console.WriteLine("Executing command: " + args[0]);
                //Console.WriteLine("With arguments: " + (args.Length > 1 ? string.Join(", ", args.Skip(1)) : "none"));
                commands[args[0]].Execute(args.Length > 1 ? args.Skip(1).ToArray() : null);

            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine("Befehl nicht gefunden: ( Fehler: " + e.Message.ToString() + ")");
            }
        }

    }
}
