namespace ProfileManager
{
    public class ConsoleOutput : IOutput
    {
        private string _controlCode;

        public Action<string> Write => (message) => WriteToConsole(message);

        public Action<string> WriteLine => (message) => WriteToConsole(message + "\n");

        public Action NewLine => () => Console.WriteLine();

        public Action WaitForUser => () => Console.ReadKey();

        private void WriteChars(string chars)
        {
            Console.Write(chars);
        }

        private void WriteEndOfLine()
        {
            Console.WriteLine();
        }

        private void WriteToConsole(string message)
        {
            for (int i = 0; i < message.Length; i++)
            {
                char c = message[i];
                if (c == '@' && _controlCode is null)
                {
                    if (message[i + 1] != '@')
                    {
                        // control code
                        string controlCode = "";

                        int j = i + 1;
                        do
                        {
                            controlCode += message[j++];
                        } while (message[j] != '{');

                        OpenControlCode(controlCode);
                        i = j;
                    }
                }
                else if (c == '}' && _controlCode is not null)
                {
                    CloseControlCode();
                }
                else
                {
                    Console.Write(c);
                }
            }
        }

        private void OpenControlCode(string code)
        {
            _controlCode = code;
            switch(code.ToLower())
            {
                case "bold":
                    break;
                case "italic":
                    break;
                default:
                    ConsoleColor color = Enum.Parse<ConsoleColor>(code, true);
                    Console.ForegroundColor = color;
                    break;
            }

        }

        private void CloseControlCode()
        {
            switch (_controlCode.ToLower())
            {
                case "bold":
                    break;
                case "italic":
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
            _controlCode = null;
        }
    }
}
