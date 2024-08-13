namespace DeviceManager
{
    public class Command
    {
        private IOutput _output;

        public Operation Operation { get; init; } = Operation.Invalid;
        public bool Debug { get; init; }
        public bool MakeBackups { get; init; } = true; // default
        public string[] Arguments { get; private set; }

        public bool HasArgument(string argumentName, string shortArgumentName)
        {
            return Arguments.Any(a => a.Matches(argumentName, shortArgumentName));
        }

        public bool HasNoArguments => Arguments.Length == 0;

        public bool TryGetArgumentValue(string argumentName, string shortArgumentName, out string value)
        {
            for (int i = 0; i < Arguments.Length; i++)
            {
                string argument = Arguments[i].ToLower();
                if (argument.Matches(argumentName, shortArgumentName))
                {
                    if (argument.Contains(':'))
                    {
                        int index = argument.IndexOf(':') + 1;
                        value = argument[index..];
                        return true;
                    }
                    else
                    {
                        value = argument.TrimStart('-');
                        return true;
                    }
                }
            }

            value = null;
            return false;
        }

        public void Execute()
        {
            switch (Operation)
            {
                case Operation.Compare:
                    Comparer comparer = new Comparer(_output);
                    comparer.Compare(this);
                    break;
                case Operation.Replace:
                    Replacer replacer = new Replacer(_output);
                    replacer.Replace(this, MakeBackups);
                    break;
                case Operation.Delete:
                    Replacer deleter = new Replacer(_output);
                    deleter.Delete(this, MakeBackups);
                    break;
                case Operation.Invalid:
                    break;
            }
        }

        public IEnumerable<string> GetProfilePaths(string path = null)
        {
            if (path is null || path == "*")
            {
                path = Environment.CurrentDirectory;
            }
            else if (!Directory.Exists(path) && File.Exists(path))
            {
                return new[] { path };
            }

            return (Directory.GetFiles(path, "*.xml")).Select(x => Path.GetFileName(x.ToLower()));
        }


        private void DisplayHelp()
        {
            _output.WriteLine("@Green{Usage: DeviceManager [--debug|-d] --operation|-o:<compare|replace|delete> [--nobackups|-nb]}");
            _output.WriteLine("  --debug|-d                  Pause for ENTER key at start to allow debugger attachment");
            _output.WriteLine("  --operation|-o:<operation>  The operation to perform.");
            _output.WriteLine("  --nobackups|-nb             Don't make backups before changing files.");
        }

        private bool RemoveArgument(string argumentName, string shortArgumentName)
        {
            bool removed = false;
            List<string> newArguments = new();
            for (int i = 0; i < Arguments.Length; i++)
            {
                string argument = Arguments[i].ToLower();
                if (!argument.Matches(argumentName, shortArgumentName))
                {
                    newArguments.Add(Arguments[i]);
                }
                else
                {
                    removed = true;
                }
            }

            Arguments = newArguments.ToArray();
            return removed;
        }

        public Command(string[] args, IOutput output)
        {
            _output = output;
            Arguments = args;
            if (args.Length > 0)
            {
                bool validArguments = false;
                bool operationSpecified = false;

                if (HasArgument("help", "h") && !HasArgument("operation", "o"))
                {
                    DisplayHelp();
                    return;
                }

                if (TryGetArgumentValue("debug", "d", out _))
                {
                    validArguments = true;
                    Debug = true;
                    RemoveArgument("debug", "d");
                    output.WriteLine("Debug mode: Attach to DeviceManager.exe process in the debugger and press ENTER");
                    output.WaitForUser();
                }

                if (TryGetArgumentValue("operation", "o", out string value))
                {
                    validArguments = true;
                    operationSpecified = true;
                    Operation = value.ToLower() switch
                    {
                        "compare" => Operation.Compare,
                        "replace" => Operation.Replace,
                        "delete" => Operation.Delete,
                        _ => Operation.Invalid
                    };
                    RemoveArgument("operation", "o");
                }

                if (TryGetArgumentValue("nobackups", "nb", out _))
                {
                    validArguments = true;
                    MakeBackups = false;
                    RemoveArgument("nobackups", "nb");
                }

                if (!validArguments || (validArguments && !operationSpecified))
                {
                    DisplayHelp();
                }
            }
            else
            {
                DisplayHelp();
            }
        }
    }
}
