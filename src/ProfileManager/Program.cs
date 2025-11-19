using ProfileManager.Output;
using System.Diagnostics;

namespace ProfileManager
{

    public class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine($"Spad.neXt Profile Manager (C)2024-{DateTime.Now.Year} Neil Hewitt");
                Console.WriteLine("Type commands below, 'quit' to exit.");
                while (true)
                {
                    Console.Write("> ");
                    string commandLine = Console.ReadLine();
                    if (commandLine.ToLower() == "quit" || commandLine.ToLower() == "q") break;
                    CommandLine command = new CommandLine(commandLine, new ConsoleOutput());
                    command.ParseAndRunCommand();
                    Console.WriteLine();
                }
            }
            else
            {
                if (args[0].ToLower() == "debug")
                {
                    args = args.Skip(1).ToArray();
                    Console.WriteLine("@DarkGray{Attach your debugger and then press ENTER to continue.}");
                    Console.ReadLine();
                }

                CommandLine command = new CommandLine(args, new ConsoleOutput());
                command.ParseAndRunCommand();
            }
        }
    }
}

