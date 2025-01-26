using ProfileManager.Output;

namespace ProfileManager.Commands
{
    public class Help : Command
    {
        public override bool TakesArguments => true;
        public override string UsageText =>
@"@Green{Usage: ProfileManager help [command]}
    [command]  Show help for a specific command.

Available commands are:
    list
    compare
    replace
    delete
";
        public void ShowHelpFor<T>() where T : Command
        {
            T command = (T)Activator.CreateInstance(typeof(T), null, new ConsoleOutput());
            command.ShowUsage();
            _output.NewLine();
            command.ShowDescription();
        }

        public override void Execute()
        {
            if (HasArguments)
            {
                // crack arguments and find appropriate help page
                string commandName = _arguments.First().Value;
                switch (commandName)
                {
                    case "list":
                        ShowHelpFor<List>();
                        break;
                    case "compare":
                        ShowHelpFor<Compare>();
                        break;
                    case "replace":
                        ShowHelpFor<Replace>();
                        break;
                }
            }
        }

        public Help(IEnumerable<Argument> arguments, IOutput output) : base(arguments, output)
        {
        }
    }
}
