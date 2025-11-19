using ProfileManager.Commands;
using ProfileManager.Comparers;
using ProfileManager.Devices;
using ProfileManager.Output;

namespace ProfileManager.Profiles
{
    public class ProfileComparer
    {
        public static void Compare(string sourcePath, string targetPath, string deviceId, string filter, string csvPath, bool noDisplay, bool verboseOutput, IOutput output)
        {
            ProfileComparer comparer = new(sourcePath, targetPath, deviceId, filter, csvPath, noDisplay, verboseOutput, output);
            comparer.Compare();
        }

        private IOutput _output;

        public string SourceProfilePath { get; private set; }
        public string TargetProfilePath { get; private set; }
        public string CSVPath { get; private set; }
        public string DeviceId { get; private set; }
        public ComparisonResult? Filter { get; private set; }
        public bool DisplayOutput { get; private set; }
        public bool VerboseOutput { get; private set; }

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
            return new ProfileComparison(source, target, DeviceId);
        }

        private void DisplayComparisons(IEnumerable<ProfileComparison> comparisons)
        {
            _output.NewLine();

            int maxLeft, maxRight;
            Dictionary<string, List<(string Left, string Right, string Comparison)>> output = new();

            foreach (ProfileComparison comparison in comparisons)
            {
                foreach (DeviceComparison deviceComparison in comparison.DeviceComparisons)
                {
                    List<(string Left, string Right, string Comparison)> outputList = new();
                    
                    if (VerboseOutput)
                    {
                        int pageIndex = 0;
                        foreach (DevicePageComparison pageComparison in deviceComparison.Pages)
                        {
                            foreach (EventComparison eventComparison in pageComparison.Events)
                            { 
                                string left = $"@Yellow{{{comparison.SourceProfile.Name}}} > {deviceComparison.DeviceID} > Page {pageIndex} > {eventComparison.EventID}";
                                string right = $"@Yellow{{{comparison.TargetProfile.Name}}}";
                                string comparisonText = 
                                    pageComparison.DoesntExist ? "@Blue{page not present}" : 
                                    eventComparison.DoesntExist ? "@Blue{not present}" : eventComparison.IsEqual ? "@Green{same}" : "@Red{different}";
                                if (Filter is null || eventComparison.Result == Filter.Value)
                                {
                                    outputList.Add((left, right, comparisonText));
                                }
                            }
                        }
                    }
                    else
                    {
                        string left = $"@Yellow{{{comparison.SourceProfile.Name}}} > {deviceComparison.DeviceID}";
                        string right = $"@Yellow{{{comparison.TargetProfile.Name}}}";
                        string comparisonText = deviceComparison.DoesntExist ? "@Blue{not present}" : deviceComparison.IsEqual ? "@Green{same}" : "@Red{different}";
                        if (Filter is null || deviceComparison.Result == Filter.Value)
                        {
                            outputList.Add((left, right, comparisonText));
                        }
                    }

                    if (outputList.Count > 0)
                    {
                        if (output.ContainsKey(deviceComparison.DeviceID))
                        {
                            output[deviceComparison.DeviceID].AddRange(outputList);
                        }
                        else
                        {
                            output.Add(deviceComparison.DeviceID, outputList);
                        }
                    }
                }
            }

            if (output.Count > 0)
            {
                var outputLines = output.OrderBy(o => o.Key).SelectMany(o => o.Value).ToList();
                maxLeft = outputLines.Max(x => x.Left.Length);
                maxRight = outputLines.Max(x => x.Right.Length);

                foreach (var outputItem in outputLines)
                {
                    _output.Write($"{outputItem.Left.PadRight(maxLeft)} -> {outputItem.Right.PadRight(maxRight)} ");
                    _output.WriteLine(outputItem.Comparison);
                }

                _output.NewLine();
            }
            else
            {
                _output.WriteLine("@Red{No comparisons to display after applying filter.}");
            }
        }

        private void WriteCSV(IEnumerable<ProfileComparison> comparisons, string csvPath)
        {
            List<string> csvData = new();
            csvData.Add("Source,Target,DeviceID,PageID,EventID,Status");
            foreach(ProfileComparison comparison in comparisons)
            {
                foreach(DeviceComparison deviceComparison in comparison.DeviceComparisons)
                {
                    foreach (DevicePageComparison pageComparison in deviceComparison.Pages)
                    {
                        foreach (EventComparison eventComparison in pageComparison.Events)
                        {
                            string status = eventComparison.DoesntExist ? "not present" : eventComparison.IsEqual ? "same" : "different";
                            if (Filter is null || eventComparison.Result == Filter.Value)
                            {
                                csvData.Add($"{comparison.SourceProfile.Name},{comparison.TargetProfile.Name},{deviceComparison.DeviceID},{pageComparison.PageID},{eventComparison.EventID},{status}");
                            }
                        }
                    }
                }
            }

            File.WriteAllLines(csvPath, csvData);
        }

        private ProfileComparer(string sourcePath, string targetPath, string deviceId, string filter, string csvPath, bool noDisplay, bool verboseOutput, IOutput output)
        {
            SourceProfilePath = sourcePath;
            TargetProfilePath = targetPath ?? Directory.GetCurrentDirectory();
            DeviceId = deviceId;
            Filter = filter is not null ? Enum.Parse<ComparisonResult>(filter, true) : null;
            CSVPath = csvPath;
            DisplayOutput = !noDisplay;
            VerboseOutput = verboseOutput;
            _output = output;
        }
    }
}
