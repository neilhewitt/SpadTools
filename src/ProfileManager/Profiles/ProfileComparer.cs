using ProfileManager.Commands;
using ProfileManager.Devices;
using ProfileManager.Output;

namespace ProfileManager.Profiles
{
    public class ProfileComparer
    {
        public static void Compare(string sourcePath, string targetPath, string deviceId, string filter, string csvPath, bool noDisplay, IOutput output)
        {
            ProfileComparer comparer = new(sourcePath, targetPath, deviceId, filter, csvPath, noDisplay, output);
            comparer.Compare();
        }

        private IOutput _output;

        public string SourceProfilePath { get; private set; }
        public string TargetProfilePath { get; private set; }
        public string CSVPath { get; private set; }
        public string DeviceId { get; private set; }
        public ComparisonResult? Filter { get; private set; }
        public bool DisplayOutput { get; private set; }

        public void Compare()
        {
            try
            {
                IEnumerable<Profile> targetProfiles = Profile.GetProfiles(TargetProfilePath, SourceProfilePath);

                if (targetProfiles.Count() == 0)
                {
                    _output.WriteLine($"No target profiles available in @Blue{{{TargetProfilePath}}}.");
                    return;
                }

                _output.WriteLine($"Using source profile @Blue{{{SourceProfilePath}}}");
                _output.WriteLine($"Using target profile {(TargetProfilePath.EndsWith(".xml") ? "" : "folder")} @Blue{{{TargetProfilePath}}}");
                if (DeviceId is not null) _output.WriteLine($"Comparing device @Red{{{DeviceId}}}");
                if (Filter is not null) _output.WriteLine($"Showing only results of type @Yellow{{{Filter.ToString().ToLower()}}}");

                Profile sourceProfile = Profile.GetProfile(SourceProfilePath);
                List<ProfileComparison> comparisons = new();

                foreach (Profile targetProfile in targetProfiles)
                {
                    comparisons.Add(DoComparison(sourceProfile, targetProfile));
                }

                if (comparisons.Count() == 0)
                {
                    _output.WriteLine("No comparisons. Device not found in any target profile, or no target profiles.");
                    return;
                }
                
                if (DisplayOutput)
                {
                    DisplayComparisons(comparisons);
                }

                if (CSVPath is not null)
                {
                    WriteCSV(comparisons, CSVPath);
                }

                _output.WriteLine("Comparison complete.");
                if (CSVPath is not null)
                {
                    _output.WriteLine($"CSV written to @Blue{{{CSVPath}}}.");
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"@Red{{{ex.Message}}}");
            }
        }

        private ProfileComparison DoComparison(Profile source, Profile target)
        {
            List<DeviceComparison> deviceComparisons = new();
            foreach (Device sourceDevice in source.Devices)
            {
                if (DeviceId is null || sourceDevice.ID == DeviceId || sourceDevice.Nickname == DeviceId)
                {
                    Device targetDevice = target.Devices.FirstOrDefault(d => 
                        d.VendorID == sourceDevice.VendorID && 
                        d.ProductID == sourceDevice.ProductID && 
                        d.DeviceIndex == sourceDevice.DeviceIndex && 
                        d.Version == sourceDevice.Version);

                    if (targetDevice is not null)
                    {
                        DeviceComparison comparison = new DeviceComparison(sourceDevice, targetDevice);
                        deviceComparisons.Add(comparison);
                    }
                }
            }

            return new ProfileComparison(source, target, deviceComparisons);
        }

        private void DisplayComparisons(IEnumerable<ProfileComparison> comparisons)
        {
            _output.NewLine();

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
                _output.Write($"{outputItem.Left.PadRight(maxLeft)} -> {outputItem.Right.PadRight(maxRight)} ");
                _output.WriteLine(outputItem.Comparison);
            }

            _output.NewLine();
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

        private ProfileComparer(string sourcePath, string targetPath, string deviceId, string filter, string csvPath, bool noDisplay, IOutput output)
        {
            SourceProfilePath = sourcePath;
            TargetProfilePath = targetPath ?? Directory.GetCurrentDirectory();
            DeviceId = deviceId;
            Filter = filter is not null ? Enum.Parse<ComparisonResult>(filter, true) : null;
            CSVPath = csvPath;
            DisplayOutput = !noDisplay;
            _output = output;
        }
    }
}
