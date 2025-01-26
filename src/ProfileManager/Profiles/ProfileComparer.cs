using ProfileManager.Commands;
using ProfileManager.Devices;
using ProfileManager.Output;

namespace ProfileManager.Profiles
{
    public class ProfileComparer
    {
        public IOutput Output { get; init; }
        public string SourceProfilePath { get; private set; }
        public string TargetProfilePath { get; private set; }
        public string CSVPath { get; private set; }
        public string DeviceId { get; private set; }
        public ComparisonResult? Filter { get; private set; }
        public bool DisplayOutput { get; private set; }

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

                TargetProfilePath = Command.GetFullPath(TargetProfilePath, out bool isFolder);
                List<string> targetProfilePaths = Command.GetProfilePaths(TargetProfilePath).ToList();
                
                targetProfilePaths.Remove(SourceProfilePath);
                if (targetProfilePaths.Count == 0)
                {
                    Output.WriteLine($"No target profiles available in @Blue{{{TargetProfilePath}}}.");
                    return;
                }

                Output.WriteLine($"Using source profile @Blue{{{SourceProfilePath}}}");
                Output.WriteLine($"Using target profile {(TargetProfilePath.EndsWith(".xml") ? "path" : "folder")} @Blue{{{TargetProfilePath}}}");
                if (DeviceId is not null) Output.WriteLine($"Comparing device @Red{{{DeviceId}}}");
                if (Filter is not null) Output.WriteLine($"Showing only results of type @Yellow{{{Filter.ToString().ToLower()}}}");

                Profile source = new Profile(SourceProfilePath, Command.DEFAULT_NICKNAMES_FILENAME);
                List<ProfileComparison> comparisons = new();

                IEnumerable<Profile> targetProfiles = Profile.GetProfiles(TargetProfilePath);
                foreach (Profile target in targetProfiles)
                {
                    comparisons.Add(new ProfileComparison(source, target, DeviceId));
                }
                
                if (DisplayOutput)
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

        private void DisplayComparisons(IEnumerable<ProfileComparison> comparisons)
        {
            Output.NewLine();

            int maxLeft, maxRight;
            List<(string Left, string Right, string Comparison)> output = new();

            foreach (ProfileComparison comparison in comparisons)
            {
                foreach (DeviceComparison deviceComparison in comparison.DeviceComparisons)
                {
                    string left = $"@Yellow{{{comparison.SourceProfile.Name}}} > {deviceComparison.DeviceID}";
                    string right = $"@Yellow{{{comparison.TargetProfile.Name}}} > {deviceComparison.DeviceID}";
                    string comparisonText = deviceComparison.DoesntExist ? "@Blue{not present}" : deviceComparison.IsEqual ? "@Green{same}" : "@Red{different}";
                    if (Filter is null || deviceComparison.Result == Filter.Value)
                    {
                        output.Add((left, right, comparisonText));
                    }
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
                    if (Filter is null || deviceComparison.Result == Filter.Value)
                    {
                        csvData.Add($"{comparison.SourceProfile.Name},{comparison.TargetProfile.Name},{deviceComparison.DeviceID},{status}");
                    }
                }
            }

            File.WriteAllLines(csvPath, csvData);
        }

        public ProfileComparer(string sourcePath, string targetPath, string device, string filter, string csvPath, bool display, IOutput output)
        {
            SourceProfilePath = sourcePath;
            TargetProfilePath = targetPath;
            DeviceId = device;
            Filter = filter is not null ? Enum.Parse<ComparisonResult>(filter, true) : null;
            CSVPath = csvPath;
            DisplayOutput = display;
            Output = output;
        }
    }
}
