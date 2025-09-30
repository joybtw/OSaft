using System;
using System.Collections.Generic;

namespace OSaft.commands
{
    public abstract class Command
    {
        internal readonly string name;
        internal Command(string name)
        {
            this.name = name;
        }

        internal abstract void Execute(string[] args);
    }
    public class CommandHelper
    {
        internal readonly static Dictionary<string, Command> commands = new();

        public static void RegisterCommand(Command command)
        {
            if (!commands.ContainsKey(command.name))
            {
                commands.Add(command.name, command);
            } else
            {
                Console.WriteLine("Befehl " + command.name  + " nicht gefunden");
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
