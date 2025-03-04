using ProfileManager.Commands;
using ProfileManager.Output;
using System.Reflection;

namespace ProfileManager
{
    public class CommandLine
    {
        private IOutput _output;

        private string _helpText =
@"@Green{Usage: ProfileManager [debug] <command-name> [--nobackups|-nb]}
    <command-name>  The operation to perform.
    --nobackups|-nb Don't make backups before changing files.

Available commands are:
    list
    compare
    replace
    delete
    help
";
        public bool MakeBackups { get; private set; } = true; // default
        public ArgumentCollection Arguments { get; init; }

        public void ParseAndRunCommand()
        {            
            Command command = MakeCommand();
            if (command is not null)
            {
                if (!command.RequiresArguments || command.HasArguments)
                {
                    command.Execute();
                }
            }
            else
            {
                _output.WriteLine(_helpText);
            }
        }

        private Command MakeCommand()
        {
            string commandName = Arguments.FirstOrDefault()?.Value.ToLower() ?? null;
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
        
        public CommandLine(string commandLine, IOutput output)
            : this(commandLine.Split(' '), output)
        {
        }

        public CommandLine(string[] args, IOutput output)
        {
            if (args.Length > 0 && !String.IsNullOrWhiteSpace(args[0]))
            {
                // handle global arguments (we'll remove them from the list once parsed)
                MakeBackups = args.Contains("--nobackups") || args.Contains("-nb");
                if (MakeBackups) args = args.Where(x => x != "--nobackups" && x != "-nb").ToArray();

                // parse the rest of the args list into a collection of Arguments
                ArgumentCollection arguments = new();

                foreach (string arg in args)
                {
                    if (!String.IsNullOrWhiteSpace(arg))
                    {
                        bool hasPathValueOnly = (arg.Length > 2 && arg[1..3] == ":\\"); // ie c:\path\to\file
                        bool hasPathValueAndName = arg.Count(c => c == ':') > 1 && !arg.Contains(":\\"); // ie --path:c:\path\to\file
                        bool hasName = !hasPathValueOnly && arg.Contains(":");

                        string[] argumentParts;
                        if (hasPathValueOnly || !hasName)
                        {
                            argumentParts = new string[] { arg };
                        }
                        else
                        {
                            argumentParts = arg.TrimStart('-').Split(':', StringSplitOptions.RemoveEmptyEntries);
                        }

                        arguments.Add(argumentParts[0], argumentParts.Length > 1 ? argumentParts[1] : null);
                    }
                }

                Arguments = arguments;
            }
            else
            { 
                Arguments = new ArgumentCollection(); 
            }

            _output = output;
        }
    }
}
