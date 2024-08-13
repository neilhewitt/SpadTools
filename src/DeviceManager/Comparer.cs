namespace DeviceManager
{
    public class Comparer
    {
        public IOutput Output { get; init; }
        public string SourceProfilePath { get; private set; }
        public string TargetProfilePath { get; private set; }
        public string CSVPath { get; private set; }

        public void Compare(Command command)
        {
            try
            {
                /* params
                 *              
                 * source (required) - the profile to compare with
                 * target (optional) - the profile to compare against (or all if not specified)
                 * csv (optional) - the path to write the CSV output to
                 * nodisplay (optional) - suppress output to the console
                 * 
                 */

                if (command.HasArgument("help", "h") || command.HasNoArguments)
                {
                    DisplayHelp();
                    return;
                }

                Output.WriteLine("Spad.neXt Profile Comparer (c)2024 Neil Hewitt");

                SourceProfilePath = command.TryGetArgumentValue("source", "s", out string sourcePath) ? sourcePath : throw new ArgumentException("Source profile path is required.");
                TargetProfilePath = command.TryGetArgumentValue("target", "t", out string targetPath) ? targetPath : "*";
                CSVPath = command.TryGetArgumentValue("csv", "c", out string csv) ? csv : null;
                bool displayOutput = !command.HasArgument("nodisplay", "nd");

                List<string> targetProfiles = command.GetProfilePaths(TargetProfilePath).ToList();
                targetProfiles.Remove(sourcePath);
                if (targetProfiles.Count == 0)
                {
                    throw new ArgumentException("No target profiles available (source and target cannot be the same).");
                }

                Profile source = new Profile(SourceProfilePath, "nicknames.txt");
                List<ProfileComparison> comparisons = new();

                foreach (string targetProfile in targetProfiles)
                {
                    Profile target = new Profile(targetProfile, "nicknames.txt");
                    comparisons.Add(new ProfileComparison(source, target));
                }
                
                if (displayOutput)
                {
                    DisplayComparisons(comparisons);
                }

                if (CSVPath is not null)
                {
                    WriteCSV(comparisons, CSVPath);
                }

                Output.WriteLine("Comparison complete.");
                if (CSVPath is not null)
                {
                    Output.WriteLine($"CSV written to @Blue{{{CSVPath}}}.");
                }
            }
            catch (Exception ex)
            {
                Output.WriteLine($"@Red{{{ex.Message}}}");
            }
        }

        private void DisplayHelp()
        {
            Output.WriteLine("@Green{Usage: DeviceManager --operation|-o:compare --source|-s:<source> [--target|-t:<target>] [--csv|-c:<path>] [-nodisplay]}");
            Output.WriteLine("  --source|-s:<source>  The profile to compare with.");
            Output.WriteLine("  --target|-t:<target>  The profile to compare against (or all if not specified).");
            Output.WriteLine("  --csv|-c:<path>        The path to write the CSV output to.");
            Output.WriteLine("  --nodisplay|-nd       Suppress output to the console.");
        }

        private void DisplayComparisons(IEnumerable<ProfileComparison> comparisons)
        {
            Output.NewLine();

            int maxLeft, maxRight;
            List<(string Left, string Right, string Comparison)> output = new();

            foreach(ProfileComparison comparison in comparisons)
            {
                foreach(DeviceComparison deviceComparison in comparison.DeviceComparisons)
                {
                    string left = $"{comparison.SourceProfile.Name}>{deviceComparison.DeviceID}";
                    string right = $"{comparison.TargetProfile.Name}>{deviceComparison.DeviceID}";
                    string comparisonText = deviceComparison.DoesntExist ? "@Blue{not present}" : deviceComparison.IsEqual ? "@Green{same}" : "@Red{different}";
                    output.Add((left, right, comparisonText));
                }
            }

            maxLeft = output.Max(x => x.Left.Length);
            maxRight = output.Max(x => x.Right.Length);

            foreach (var outputItem in output)
            {
                Output.Write($"{outputItem.Left.PadRight(maxLeft)} -> {outputItem.Right.PadRight(maxRight)} ");
                Output.WriteLine(outputItem.Comparison);
            }

            Output.NewLine();
        }

        private void WriteCSV(IEnumerable<ProfileComparison> comparisons, string csvPath)
        {
            List<string> csvData = new();
            csvData.Add("Source,Target,DeviceID,Status");
            foreach(ProfileComparison comparison in comparisons)
            {
                foreach(DeviceComparison deviceComparison in comparison.DeviceComparisons)
                {
                    string status = deviceComparison.DoesntExist ? "not present" : deviceComparison.IsEqual ? "same" : "different";
                    csvData.Add($"{comparison.SourceProfile.Name},{comparison.TargetProfile.Name},{deviceComparison.DeviceID},{status}");
                }
            }

            File.WriteAllLines(csvPath, csvData);
        }

        public Comparer(IOutput output)
        {
            Output = output;
        }
    }
}
