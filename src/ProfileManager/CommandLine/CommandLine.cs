using ProfileManager.Commands;
using ProfileManager.Output;
using System.Reflection;

namespace ProfileManager
{
    public class CommandLine
    {
        private IOutput _output;
        private Dictionary<string, Argument> _arguments;

        private string _helpText =
@"@Green{Usage: ProfileManager [debug] <command-name> [--nobackups|-nb]}
    debug           Pause for ENTER key at start to allow debugger attachment
    <command-name>  The operation to perform.
    --nobackups|-nb Don't make backups before changing files.

Available commands are:
    list
    compare
    replace
    delete
    help
";

        public bool Debug { get; private set; } = false; // default
        public bool MakeBackups { get; private set; } = true; // default
        public IEnumerable<Argument> Arguments => _arguments.Values;

        public bool TryGetArgument(string argumentName, string shortArgumentName, out string value)
        {
            if (_arguments.ContainsKey(argumentName.ToLower()))
            {
                value = _arguments[argumentName.ToLower()].Value;
                return true;
            }
            else if (_arguments.ContainsKey(shortArgumentName.ToLower()))
            {
                value = _arguments[shortArgumentName.ToLower()].Value;
                return true;
            }

            value = null;
            return false;
        }

        public void ParseAndRunCommand()
        {            
            if (Debug)
            {
                _output.WriteLine($"Debug mode: Attach to ProfileManager.exe process in the debugger, then {_output.WaitPrompt} to continue");
                _output.WaitForUser();
            }

            Command command = MakeCommand();
            if (command is not null)
            {
                if (!command.TakesArguments || command.HasArguments)
                {
                    command.Execute();
                }
            }
            else
            {
                ShowHelp();
            }
        }

        private Command MakeCommand()
        {
            string commandName = Arguments?.FirstOrDefault()?.Value.ToLower() ?? null;
            return (commandName) switch
            {
                "list" => new List(Arguments, _output),
                "help" => new Help(Arguments, _output),
                "compare" => new Compare(Arguments, _output),
                "replace" => new Replace(Arguments, _output, MakeBackups),
                "delete" => new Delete(Arguments, _output, MakeBackups),
                _ => null
            };
        }
        
        private void ShowHelp()
        {
            _output.WriteLine(_helpText);
        }

        public CommandLine(string commandLine, IOutput output)
            : this(commandLine.Split(' '), output)
        {
        }

        public CommandLine(string[] args, IOutput output)
        {
            // handle global arguments (remove from list once parsed)
            Debug = args[0].ToLower() == "debug";
            if (Debug) args = args.Skip(1).ToArray();

            MakeBackups = args.Contains("--nobackups") || args.Contains("-nb");
            if (MakeBackups) args = args.Where(x => x != "--nobackups" && x != "-nb").ToArray();

            // parse the rest of the args list into a collection of Arguments to form the command line
            if (args.Length > 0 && args[0] != "")
            {
                List<Argument> arguments = new();
                Argument current = null, previous = null;

                foreach (string arg in args)
                {
                    // have to handle args that are file paths
                    string[] argumentParts = arg[1..3] == ":\\" ? new string[] { arg } : arg.TrimStart('-').Split(':', StringSplitOptions.RemoveEmptyEntries);

                    previous = current;
                    current = new Argument(argumentParts[0], argumentParts.Length > 1 ? argumentParts[1] : null) { Previous = previous };
                    if (previous is not null)
                    {
                        previous.Next = current;
                    }
                    
                    arguments.Add(current);
                }

                _arguments = new(arguments.ToDictionary(arguments => arguments.Name.ToLower()));
            }
            else
            {
                _arguments = new(); // empty
            }

            _output = output;
        }
    }
}
