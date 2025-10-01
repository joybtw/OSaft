using System;
using System.Collections.Generic;

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

        public static void ExecuteCommand(string key, string[] args)
        {
            try
            {
                commands[key].Execute(args);
            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine("Befehl nicht gefunden: (" + e.Message.ToString() + ")");
            }
        }

    }
}
