using ProfileManager.Output;

namespace ProfileManager.Commands
{
    public class Help : Command
    {
        public override bool RequiresArguments => true;
        public override string UsageText =>
@"@Green{Usage: ProfileManager help [command]}
    [command]  Show help for a specific command.

Available commands are:
    list
    compare
    replace
    delete
";
        public void ShowHelpFor<T>(params object[] extraParameters) where T : Command
        {
            object[] parameters = [null, new ConsoleOutput()];
            parameters = parameters.Concat(extraParameters).ToArray();

            T command = (T)Activator.CreateInstance(typeof(T), parameters);
            command.ShowDescription();
        }

        public override void Execute()
        {
            if (HasArguments)
            {
                // crack arguments and find appropriate help page
                string commandName = Arguments.First().Value;
                switch (commandName)
                {
                    case "list":
                        ShowHelpFor<List>();
                        break;
                    case "compare":
                        ShowHelpFor<Compare>();
                        break;
                    case "replace":
                        ShowHelpFor<Replace>(false);
                        break;
                    case "delete":
                        ShowHelpFor<Delete>(false);
                        break;
                }
            }
        }

        public Help(ArgumentCollection arguments, IOutput output) : base(arguments, output)
        {
        }
    }
}
