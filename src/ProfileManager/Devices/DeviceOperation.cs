using ProfileManager.Commands;
using ProfileManager.Output;
using ProfileManager.Profiles;

namespace ProfileManager.Devices
{

    public class DeviceOperation    
    {
        public static void Delete(string targetPath, string deviceId, string customEventId, IOutput output, bool makeBackups)
        {
            DeviceOperation deviceOperation = new(DeviceOperationType.Delete, null, targetPath, deviceId, customEventId, output);
            deviceOperation.Execute(makeBackups);
        }

        public static void Replace(string sourceProfileName, string targetPath, string deviceId, string customEventId, IOutput output, bool makeBackups)
        {
            DeviceOperation deviceOperation = new(DeviceOperationType.Replace, sourceProfileName, targetPath, deviceId, customEventId, output);
            deviceOperation.Execute(makeBackups);
        }

        private IOutput _output;
        private Profile _sourceProfile;
        private IEnumerable<Profile> _targetProfiles;

        public string SourceProfileName { get; private set; }
        public string TargetPath { get; private set; }
        public string DeviceId { get; private set; }
        public string CustomEventId { get; private set; }
        public DeviceOperationType Type { get; private set; }

        private void Execute(bool makeBackups)
        {
            try
            {
                if (Type == DeviceOperationType.Replace && (SourceProfileName is null || !File.Exists(SourceProfileName)))
                {
                    throw new ArgumentException("Source profile is required or specified profile does not exist.");
                }

                if (makeBackups) _output.WriteLine("@Gray{Backups will be made for any changed profiles.}");

                foreach (Profile targetProfile in _targetProfiles)
                {
                    bool done = false;
                    bool isDelete = Type == DeviceOperationType.Delete;

                    Device device = (_sourceProfile ?? targetProfile).GetDevice(DeviceId);
                    if (device is null)
                    {
                        _output.WriteLine($"Device @Red{{{DeviceId}}} not found in @Green{{{(_sourceProfile ?? targetProfile).Name}}}.");
                        return;
                    }

                    Event sourceEvent = CustomEventId is not null ? device.GetEvent(CustomEventId) : null;

                    string status = isDelete ? "Deleting " : "Replacing ";
                    if (CustomEventId is not null) status += $"event @Magenta{{{sourceEvent.BoundTo}}} in ";
                    status += $"device @Red{{{device.Nickname ?? device.ID}}} in @Green{{{targetProfile.Name}}}";
                    if (!isDelete) status += $" from @Blue{{{_sourceProfile.Name}}}";
                    status += "... ";
                    _output.Write(status);

                    if (makeBackups) targetProfile.Backup();

                    if (CustomEventId is not null)
                    {
                        // event mode
                        Device targetDevice = targetProfile.GetDevice(DeviceId);
                        if (targetDevice is not null)
                        {
                            if (sourceEvent is not null)
                            {
                                if (isDelete)
                                {
                                    targetDevice.DeleteEvent(CustomEventId);
                                }
                                else
                                {
                                    targetDevice.ReplaceEvent(CustomEventId, sourceEvent);
                                }

                                done = true;
                            }
                            else
                            {
                                _output.NewLine();
                                _output.WriteLine($"@Red{{Fatal error}}: Event @Magenta{{{CustomEventId}}} not found in @Red{{{device.Nickname}}} in @Blue{{{_sourceProfile.Name}}}.");
                                return;
                            }
                        }
                        else
                        {
                            _output.WriteLine($"@@Red{{device not found}}, skipping.");
                        }
                    }
                    else
                    {
                        // whole device mode
                        if (isDelete)
                        {
                            if (!targetProfile.DeleteDevice(device))
                            {
                                _output.WriteLine("device does not exist in this profile.");
                            }
                        }
                        else
                        {
                            if (!targetProfile.ReplaceDevice(device))
                            {
                                _output.WriteLine("device does not exist in this profile.");
                            }
                        }

                        done = true;
                    }

                    if (done)
                    {
                        targetProfile.Save();
                        _output.WriteLine("done.");
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"@Red{{{ex.Message}}}");
            }
        }

        private DeviceOperation(DeviceOperationType type, string sourceProfileName, string targetPath, string deviceId, string customEventId, IOutput output)
        {
            Type = type;
            SourceProfileName = sourceProfileName;
            TargetPath = targetPath;
            DeviceId = deviceId;
            CustomEventId = customEventId;
            _output = output;

            IEnumerable<Profile> targetProfiles = Profile.GetProfiles(TargetPath);
            if (targetProfiles.Count() == 0)
            {
                _output.WriteLine($"No target profiles available in @Blue{{{TargetPath}}}");
                throw new ArgumentException($"No target profiles available in ${TargetPath}.");
            }

            _targetProfiles = targetProfiles;
            _sourceProfile = SourceProfileName is not null ? Profile.GetProfile(SourceProfileName) : null;
        }
    }
}
