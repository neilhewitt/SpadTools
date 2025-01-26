using ProfileManager.Output;

namespace ProfileManager
{

    public class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Spad.neXt Profile Manager (C)2024 Neil Hewitt");
                Console.WriteLine("Type commands below, 'quit' to exit.");
                while (true)
                {
                    Console.Write("> ");
                    string commandLine = Console.ReadLine();
                    if (commandLine.ToLower() == "quit" || commandLine.ToLower() == "q") break;
                    CommandLine command = new CommandLine(commandLine, new ConsoleOutput());
                    command.ParseAndRunCommand();
                }
            }
            else
            {
                CommandLine command = new CommandLine(args, new ConsoleOutput());
                command.ParseAndRunCommand();
            }
        }
    }
}

