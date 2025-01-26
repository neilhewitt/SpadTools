using ProfileManager.Commands;
using ProfileManager.Output;
using ProfileManager.Profiles;
using System.Linq.Expressions;

namespace ProfileManager.Devices
{

    public class DeviceReplacer
    {
        private IOutput Output { get; }

        public string SourceProfilePath { get; private set; }
        public string TargetProfilePath { get; private set; }
        public string DeviceId { get; private set; }
        public string EventBoundTo { get; private set; }    

        public void Replace(Command command, bool makeBackups)
        {
            ReplaceOrDelete(command, makeBackups, false);
        }

        public void Delete(Command command, bool makeBackups)
        {
            ReplaceOrDelete(command, makeBackups, true);
        }

        private void ReplaceOrDelete(Command command, bool makeBackups, bool deleteNotReplace)
        {
            try
            {
                SetupReplacements(command, SourceProfilePath, TargetProfilePath, makeBackups, out IList<string> targetProfiles);
                DoReplacements(SourceProfilePath, targetProfiles, makeBackups, deleteNotReplace);
            }
            catch (Exception ex)
            {
                Output.WriteLine($"@Red{{{ex.Message}}}");
            }
        }

        private void SetupReplacements(Command command, string sourceProfilePath, string targetProfilePath, bool makeBackups, out IList<string> targetProfiles)
        {
            targetProfiles = Command.GetProfilePaths(TargetProfilePath).ToList();
            if (sourceProfilePath is not null) targetProfiles.Remove(sourceProfilePath);
            if (targetProfiles.Count == 0)
            {
                throw new ArgumentException($"No target profiles available in @Blue{{${targetProfilePath}}}.");
            }

            if (makeBackups) Output.WriteLine("@Gray{Backups will be made for any changed profiles.}");
        }

        private void DoReplacements(string source, IEnumerable<string> targets, bool makeBackups, bool deleteNotReplace)
        {
            Profile sourceProfile = String.IsNullOrWhiteSpace(source) ? null : new Profile(SourceProfilePath, Command.DEFAULT_NICKNAMES_FILENAME);
            foreach (string target in targets)
            {
                bool done = false;

                Profile targetProfile = new Profile(target, Command.DEFAULT_NICKNAMES_FILENAME);
                Device device = GetDeviceById(sourceProfile ?? targetProfile, DeviceId);

                if (device is null)
                {
                    Output.WriteLine($"Device @Red{{{DeviceId}}} not found in @Green{{{sourceProfile.Name}}}.");
                    return;
                }

                Event sourceEvent = EventBoundTo is not null ? device.GetEvent(EventBoundTo) : null;
                Output.Write(getStatus()); // this is a complex message so I made a local function for it
                                    
                if (makeBackups) targetProfile.Backup();

                if (EventBoundTo is not null)
                {
                    // event mode
                    Device targetDevice = GetDeviceById(targetProfile, DeviceId);
                    if (targetDevice is not null)
                    {
                        if (sourceEvent is not null)
                        {
                            if (deleteNotReplace)
                            {
                                targetDevice.DeleteEvent(EventBoundTo);
                            }
                            else
                            {
                                targetDevice.ReplaceEvent(EventBoundTo, sourceEvent);
                            }

                            done = true;
                        }
                        else
                        {
                            Output.NewLine();
                            Output.WriteLine($"@Red{{Fatal error}}: Event @Magenta{{{EventBoundTo}}} not found in @Red{{{device.Nickname}}} in @Blue{{{sourceProfile.Name}}}.");
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
                    if (EventBoundTo is not null)
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

        private Device GetDeviceById(Profile profile, string DeviceId)
        {
            if (DeviceId.Contains(','))
            {
                // assume device id format
                string[] parts = DeviceId.Split(',');
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
                return profile.GetDevice(DeviceId);
            }
        }

        public DeviceReplacer(string sourcePath, string targetPath, string deviceId, string eventBoundTo, IOutput output)
        {
            SourceProfilePath = sourcePath;
            TargetProfilePath = targetPath;
            DeviceId = deviceId;
            EventBoundTo = eventBoundTo;
            Output = output;
        }
    }
}
