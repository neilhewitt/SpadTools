namespace ProfileManager
{

    public class Program
    {
        private static void Main(string[] args)
        {
            //Command command = new Command(new string[] { "-o:replace", @"-s:c:\temp\profiles\base_profile.xml", @"-t:c:\temp\profiles\737-900.xml", "-d:Virpil_Stick" }, new ConsoleOutput());
            Command command = new Command(args, new ConsoleOutput());
            command.Execute();
        }
    }
}

