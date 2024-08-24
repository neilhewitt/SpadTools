using System.Security.Cryptography.X509Certificates;

namespace ProfileManager
{
    public class Replacer
    {
        private IOutput Output { get; }

        public string SourceProfilePath { get; private set; }
        public string TargetProfilePath { get; private set; }

        public void Replace(Command command, bool makeBackups)
        {
            try
            {
                SourceProfilePath = command.TryGetArgumentValue("source", "s", out string sourcePath) ? sourcePath : throw new ArgumentException("Source profile path is required.");
                TargetProfilePath = command.TryGetArgumentValue("target", "t", out string targetPath) ? targetPath : "*";

                SetupReplacements(command, SourceProfilePath, TargetProfilePath, makeBackups, out IList<string> targetProfiles, out string deviceId, out string eventBoundTo);
                DoReplacements(SourceProfilePath, targetProfiles, deviceId, eventBoundTo, makeBackups, false);
            }
            catch (Exception ex)
            {
                Output.WriteLine($"@Red{{{ex.Message}}}");
                DisplayHelp();
            }
        }

        public void Delete(Command command, bool makeBackups)
        {
            try
            {
                TargetProfilePath = command.TryGetArgumentValue("target", "t", out string targetPath) ? targetPath : "*";

                SetupReplacements(command, null, TargetProfilePath, makeBackups, out IList<string> targetProfiles, out string deviceId, out string eventBoundTo);
                DoReplacements(null, targetProfiles, deviceId, eventBoundTo, makeBackups, true);
            }
            catch (Exception ex)
            {
                Output.WriteLine($"@Red{{{ex.Message}}}");
                DisplayDeleteHelp();
            }
        }

        private void SetupReplacements(Command command, string sourceProfilePath, string targetProfilePath, bool makeBackups, out IList<string> targetProfiles, out string deviceId, out string eventBoundTo)
        {
            targetProfiles = command.GetProfilePaths(TargetProfilePath).ToList();
            if (sourceProfilePath is not null) targetProfiles.Remove(sourceProfilePath);
            if (targetProfiles.Count == 0)
            {
                throw new ArgumentException("No target profiles available (source and target cannot be the same).");
            }

            deviceId = command.TryGetArgumentValue("device", "d", out string device) ? device : throw new ArgumentException("Device ID or nickname is required.");
            eventBoundTo = command.TryGetArgumentValue("event", "e", out string boundTo) ? boundTo : null;

            Output.WriteLine("Spad.neXt Profile Device Replacer (c)2024 Neil Hewitt");
            if (makeBackups) Output.WriteLine("@Gray{Backups will be made for any changed profiles.}");
        }

        private void DoReplacements(string source, IEnumerable<string> targets, string deviceId, string eventBoundTo, bool makeBackups, bool deleteNotReplace)
        {
            Profile sourceProfile = String.IsNullOrWhiteSpace(source) ? null : new Profile(SourceProfilePath, "nicknames.txt");
            foreach (string target in targets)
            {
                bool done = false;

                Profile targetProfile = new Profile(target, "nicknames.txt");
                Device device = GetDeviceById(sourceProfile ?? targetProfile, deviceId);

                if (device is null)
                {
                    Output.Write($"Device @Red{{{deviceId}}} not found in @Green{{{sourceProfile.Name}}}.");
                    return;
                }

                Event sourceEvent = eventBoundTo is not null ? device.GetEvent(eventBoundTo) : null;
                Output.Write(getStatus()); // this is a complex message so I made a local function for it
                                    
                if (makeBackups) targetProfile.Backup();

                if (eventBoundTo is not null)
                {
                    // event mode
                    Device targetDevice = GetDeviceById(targetProfile, deviceId);
                    if (targetDevice is not null)
                    {
                        if (sourceEvent is not null)
                        {
                            if (deleteNotReplace)
                            {
                                targetDevice.DeleteEvent(eventBoundTo);
                            }
                            else
                            {
                                targetDevice.ReplaceEvent(eventBoundTo, sourceEvent);
                            }

                            done = true;
                        }
                        else
                        {
                            Output.NewLine();
                            Output.WriteLine($"@Red{{Fatal error}}: Event @Magenta{{{eventBoundTo}}} not found in @Red{{{device.Nickname}}} in @Blue{{{sourceProfile.Name}}}.");
                            return;
                        }
                    }
                    else
                    {
                        Output.WriteLine($"@@Red{{device not found}}, skipping.");
                    }
                }
                else
                {
                    // whole device mode
                    if (deleteNotReplace)
                    {
                        targetProfile.DeleteDevice(device);
                    }
                    else
                    {
                        targetProfile.ReplaceDevice(device);
                    }

                    done = true;
                }

                if (done)
                {
                    targetProfile.Save();
                    Output.WriteLine("done.");
                }

                string getStatus()
                {
                    string status = deleteNotReplace ? "Deleting" : "Replacing";
                    if (eventBoundTo is not null)
                    {
                        status += $" event @Magenta{{{sourceEvent.BoundTo}}} in";
                    }
                    status += $" device @Red{{{device.Nickname}}} in @Green{{{targetProfile.Name}}}";
                    if (!deleteNotReplace)
                    {
                        status += $" from @Blue{{{sourceProfile.Name}}}";
                    }
                    status += "... ";

                    return status;
                }
            }
        }

        private Device GetDeviceById(Profile profile, string deviceId)
        {
            if (deviceId.Contains(','))
            {
                // assume device id format
                string[] parts = deviceId.Split(',');
                if (parts.Length < 2)
                {
                    throw new ArgumentException("Invalid device ID format.");
                }

                string vendorId = parts[0];
                string productId = parts[1];
                int deviceIndex = parts.Length > 2 ? int.Parse(parts[2]) : -1;
                int version = parts.Length > 3 ? int.Parse(parts[3]) : -1;

                return profile.GetDevice(vendorId, productId, deviceIndex, version);
            }
            else
            {
                // assume nickname
                return profile.GetDevice(deviceId);
            }
        }

        private void DisplayHelp()
        {
            Output.WriteLine("@Green{Usage: ProfileManager --operation|-o:replace --source|-s:<source> [--target|-t:<target>]}");
            Output.WriteLine("@Green{                     --device|-dev:<nickname>|<vendorId,productId[,deviceIndex][,version]}");
            Output.WriteLine("  --source|-s:<source>   The profile to replace from.");
            Output.WriteLine("  --target|-t:<target>   The profile to replace into (or all if not specified).");
            Output.WriteLine("  --device|-d:<device>   The device to replace.");
            Output.WriteLine("                         @Yellow{Specify either by device nickname (listed in nicknames.txt)}");
            Output.WriteLine("                         @Yellow{or by a comma-delimited string giving vendorId and productId}");
            Output.WriteLine("                         @Yellow{(and optionally device index and version)}.");
            Output.WriteLine("  --event:-e:<boundTo>   Replace just event bound to <boundTo>.");
        }

        private void DisplayDeleteHelp()
        {
            Output.WriteLine("@Green{Usage: ProfileManager --operation|-o:delete [--target|-t:<target>]}");
            Output.WriteLine("@Green{                     --device|-dev:<nickname>|<vendorId,productId[,deviceIndex][,version]}");
            Output.WriteLine("  --target|-t:<target>   The profile to delete from (or all if not specified).");
            Output.WriteLine("  --device|-d:<device>   The device to delete.");
            Output.WriteLine("                         @Yellow{Specify either by device nickname (listed in nicknames.txt)}");
            Output.WriteLine("                         @Yellow{or by a comma-delimited string giving vendorId and productId}");
            Output.WriteLine("                         @Yellow{(and optionally device index and version)}.");
            Output.WriteLine("  --event:-e:<boundTo>   Delete just event bound to <boundTo>.");
        }

        public Replacer(IOutput output)
        {
            Output = output;
        }
    }
}
