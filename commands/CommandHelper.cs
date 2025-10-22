using System;
using System.Collections.Generic;
using System.Linq;

namespace OSaft.commands
{
    public abstract class Command
    {
        public string Name { get; private set;  }
        public string Description { get; private set;  }
        public List<CommandParameter> ParameterList { get; protected set; } //Erstellen einer Liste "ParameterList" mit CommandParameter-Objekten

        public Command(string name, string description)
        {
            this.Name = name;
            this.Description = description;
            void help(string val)
            {
                Console.WriteLine(this.Name + ": " + this.Description);
                Console.WriteLine("Syntax: " + GetSyntax());
            }
            this.ParameterList = new List<CommandParameter> {
                    new("-h", "Displays help", false, false, help),
                    new("--help", "Displays help", false, false, help)
                };
            DefineParameters();
        }

        //die Methode "HasRequiredParamCount" soll überprüfen, ob alle verpflichtenden Parameter für diesen Befehl auch tatsächlich angegeben wurden
        //wenn die Parameter passen, gib einen bool "true" zurück
        internal bool HasRequiredParamCount(string[] args)
        {
            if (args == null) { return true; }
            int reqCnt = 0;
            foreach (CommandParameter parameter in ParameterList)
            {
                if (parameter.IsRequired) { reqCnt++; }
            }
            return args.Length >= reqCnt;
        }

        public string GetSyntax()
        {
            string result = this.Name ?? "";

            foreach (CommandParameter parameter in this.ParameterList)
            {
                result += " <" + parameter.Name + ">";
            }

            return result;
        }

        internal void ExecuteParams(string[] args)
        {
            if (args == null || args.Length == 0) { return; }
            for (int i = 0; i < args.Length; i++)
            {
                string current = args[i];
                CommandParameter param = ParameterList.FirstOrDefault(p => p.Name == current);
                param ??= ParameterList.ElementAt(i);

                if (param != null)
                {
                    string value = null;
                    if (param.ExpectsParameters && i + 1 < args.Length)
                    {
                        value = args[i + 1];
                        i++; // überspringe den Wert beim nächsten Durchlauf
                    } else
                    {
                        value = args[i];
                    }
                    param.Action?.Invoke(value);
                }
            }
        }

        internal virtual void Execute(string[] args)
        {
            ExecuteParams(args);
        }

        //definition einer methode DefineParameters, um in erbenden Klassen (z.b. RMCommand) die Parameter zu registrieren
        //virtual: methode tut im standardfall nichts, kann aber aus erbenden Klassen überschrieben werden: protected override void DefineParameters() { }
        protected virtual void DefineParameters() { }
    }

    //-------------------------------------------------------------------------

    public class CommandParameter
    {
        //Definition Properties
        public string Name { get; }
        public string Description { get; }
        public bool IsRequired { get; }
        public bool ExpectsParameters { get; }
        public Action<string> Action { get; }
        

        public CommandParameter(string name, string description, bool isRequired, bool expectsParameters, Action<string> action) //Konstruktor
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Missing parameters detected");
            //sicherstellen, dass name und description gesetzt sind
            this.Name = name;
            this.Description = description;
            this.IsRequired = isRequired;
            this.ExpectsParameters = expectsParameters;
            this.Action = action;
        }
    }

    //-------------------------------------------------------------------------


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
                Console.WriteLine("Command " + command.Name + " already exists.");
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
                Command cmd = commands[args[0]];
                string[] cmdArgs = args.Length > 1 ? args.Skip(1).ToArray() : Array.Empty<string>();
                if (cmd.HasRequiredParamCount(cmdArgs))
                {
                    cmd.Execute(cmdArgs);
                }
                else //Fehlermeldung: Syntax für aktuellen Command
                { 
                    Console.WriteLine("Syntax: " + cmd.GetSyntax());
                }
            }
            catch (KeyNotFoundException e)
            {
                Console.WriteLine("Befehl nicht gefunden: ( Fehler: " + e.Message.ToString() + ")");
            }
        }

    }
}
