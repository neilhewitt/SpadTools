using NewProfileManager.Commands;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Reflection.Metadata.Ecma335;

namespace NewProfileManager
{
    public class Program
    {
        private static CommandApp _app = new CommandApp();
        static int Main(string[] args)
        {
            AnsiConsole.MarkupLine("[bold]Spad.neXt Profile Manager (C)2025 Neil Hewitt[/]\n");
            SetupCommands();

            if (args.Length == 0)
            {
                while (true)
                {
                    AnsiConsole.MarkupLine("[green]Enter command below, or q to quit[/]");                    
                    string input = AnsiConsole.Ask<string>("> ").Trim();
                    if (input == "quit" || input == "q")
                    {
                        return 0;
                    }
                    
                    args = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    ExecuteCommand(args);
                }
            }
            else
            {
                return ExecuteCommand(args);
            }
        }

        private static int ExecuteCommand(string[] args)
        {
            try
            {
                return _app.Run(args);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/red]");
                return 1;
            }
        }

        private static void SetupCommands()
        {
            _app.Configure(config =>
            {
                config.AddCommand<ListCommand>("list");
            });
        }
    }
}
