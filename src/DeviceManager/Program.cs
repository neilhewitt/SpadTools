namespace DeviceManager
{

    public class Program
    {
        private static void Main(string[] args)
        {
            Command command = new Command(args, new ConsoleOutput());
            command.Execute();
        }
    }
}

