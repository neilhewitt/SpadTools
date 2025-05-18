using ProfileManager.Commands;
using ProfileManager.Output;
using System.Reflection;

namespace ProfileManager
{
    public class CommandLine
    {
        private IOutput _output;

        private string _helpText =
@"@Green{Usage: ProfileManager <command-name> [--nobackups|-nb]}
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
        public ArgumentCollection Arguments { get; private set; }

        public void ParseAndRunCommand()
        {
            try
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
            catch
            {
                ErrorIfAllElseFails();
            }
        }

        private Command MakeCommand()
        {
            string commandName = Arguments.FirstOrDefault()?.Value.ToLower() ?? null;
            return (commandName) switch
            {
                "list" => new List(Arguments, _output),
                "help" => new Help(Arguments, _output),
                "?" => new Help(Arguments, _output),
                "compare" => new Compare(Arguments, _output),
                "replace" => new Replace(Arguments, _output, MakeBackups),
                "delete" => new Delete(Arguments, _output, MakeBackups),
                _ => null
            };
        }

        private void ErrorIfAllElseFails()
        {
            string message = "Something unexpected went wrong. Please check your arguments.";
            if (_output is not null)
            {
                _output.WriteLine($"@Red{{{message}}}");
            }
            else
            {
                Console.WriteLine(message); // ultimate fallback
            }
        }

        private void Setup(string[] args, IOutput output)
        {
            try
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
            catch
            {
                ErrorIfAllElseFails();
            }
        }

        public CommandLine(string commandLine, IOutput output)
        {
            // if we get the command line from a user entry, we need to turn quoted strings into single arguments
            // because otherwise spaces in the quoted string will be treated as argument separators
            bool inQuotedString = false;
            string fixedCommandLine = "";
            foreach(char c in commandLine)
            {
                if (c == '"')
                {
                    inQuotedString = !inQuotedString;
                }
                else if (inQuotedString && c == ' ')
                {
                    fixedCommandLine += "$$SPACE$$"; // need a token that won't naturally occur in any arg value (probably)
                }
                else
                {
                    fixedCommandLine += c;
                }
            }

            List<string> args = new List<string>();
            string[] fixedArgs = fixedCommandLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(string fixedArg in fixedArgs)
            {
                args.Add(fixedArg.Replace("$$SPACE$$", " "));
            }

            // now we can send the args to the setup method like the other constructor
            // (the quotes have been stripped from the value)
            Setup(args.ToArray(), output);
        }

        public CommandLine(string[] args, IOutput output)
        {
            // if we get the args directly from the console, they will contain quoted strings correctly
            Setup(args, output);
        }
    }
}
