namespace DeviceManager
{
    public class CSVOutput : IOutput
    {
        private string _csvPath;
        private string _csvLine;
        private List<string> _csvData = new();

        public Action<string> Write => (message) => WriteToCSV(message, false);

        public Action<string> WriteLine => (message) => WriteToCSV(message, true);

        public Action NewLine => () => { };

        public Action WaitForUser => () => { };

        private void WriteToCSV(string message, bool newLineAfter)
        {
            _csvLine += message + ",";
            if (newLineAfter)
            {
                _csvData.Add(_csvLine.TrimEnd(',')); // remove trailing comma
                _csvLine = "";
            }
        }

        public CSVOutput(string csvPath)
        {
            _csvPath = csvPath;
        }
    }
}
