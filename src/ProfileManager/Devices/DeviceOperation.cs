using ProfileManager.Commands;
using ProfileManager.Output;
using ProfileManager.Profiles;
using System.Diagnostics.Tracing;

namespace ProfileManager.Devices
{

    public class DeviceOperation    
    {
        public static void Delete(string targetPath, string deviceId, string[] eventIds, IOutput output, bool makeBackups)
        {
            DeviceOperation deviceOperation = new(DeviceOperationType.Delete, null, targetPath, deviceId, eventIds, output);
            deviceOperation.Execute(makeBackups);
        }

        public static void Replace(string sourceProfileName, string targetPath, string deviceId, string[] eventIds, IOutput output, bool makeBackups)
        {
            DeviceOperation deviceOperation = new(DeviceOperationType.Replace, sourceProfileName, targetPath, deviceId, eventIds, output);
            deviceOperation.Execute(makeBackups);
        }

        private IOutput _output;
        private Profile _sourceProfile;
        private IEnumerable<Profile> _targetProfiles;

        public string SourceProfileName { get; private set; }
        public string TargetPath { get; private set; }
        public string DeviceId { get; private set; }
        public IEnumerable<string> EventIds { get; private set; }
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

                    Device sourceDevice = _sourceProfile?.GetDevice(DeviceId);
                    if (!targetProfile.TryGetDevice(DeviceId, out Device targetDevice) && sourceDevice != null)
                    {
                        // create a new device if it doesn't exist in the target profile
                        targetDevice = new Device(sourceDevice.Node, targetProfile);

                        // remove all events except the ones in EventIds unless it's empty
                        if (EventIds?.Count() > 0)
                        {
                            foreach (DevicePage page in targetDevice.Pages)
                            {
                                IEnumerable<Event> eventsToRemove = page.Events.Where(e => !EventIds.Contains(e.BoundTo, StringComparer.OrdinalIgnoreCase)).ToList();
                                foreach (Event eventToRemove in eventsToRemove)
                                {
                                    targetDevice.DeleteEvent(eventToRemove.BoundTo);
                                }
                            }
                        }
                    }

                    if (targetDevice is null)
                    {
                        _output.WriteLine($"Device @Red{{{DeviceId}}} not found in @Green{{{(_sourceProfile ?? targetProfile).Name}}}.");
                        return;
                    }

                    if (makeBackups) targetProfile.Backup();

                    if (EventIds?.Count() > 0)
                    {
                        // event mode
                        foreach(string eventId in EventIds)
                        {
                            if (eventId == null)
                            {
                                _output.WriteLine("@Red{Invalid event ID specified, skipping.}");
                                continue;
                            }

                            Event sourceEvent = sourceDevice.GetEvent(eventId);
                            if (sourceEvent == null)
                            {
                                _output.WriteLine($"@Red{{Fatal error}}: Event @Magenta{{{eventId}}} not found in @Red{{{sourceDevice.Nickname}}} in @Blue{{{(_sourceProfile ?? targetProfile).Name}}}.");
                                return;
                            }

                            bool isAdd = !isDelete && targetDevice.GetEvent(eventId) == null;
                            string status = isAdd ? "Adding " : isDelete ? "Deleting " : "Replacing ";
                            status += $"event @Magenta{{{sourceEvent.BoundTo}}} in ";
                            status += $"device @Red{{{sourceDevice.Nickname ?? sourceDevice.ID}}} in @Green{{{targetProfile.Name}}}";
                            if (!isDelete) status += $" from @Blue{{{_sourceProfile.Name}}}";
                            status += "... ";
                            _output.Write(status);

                            if (isDelete)
                            {
                                targetDevice.DeleteEvent(eventId);
                            }
                            else
                            {
                                targetDevice.ReplaceEvent(eventId, sourceEvent);
                            }

                            _output.WriteLine("done.");
                            done = true;
                        }
                    }
                    else
                    {
                        string status = isDelete ? "Deleting " : "Replacing ";
                        status += $"device @Red{{{sourceDevice.Nickname ?? sourceDevice.ID}}} in @Green{{{targetProfile.Name}}}";
                        if (!isDelete) status += $" from @Blue{{{_sourceProfile.Name}}}";
                        status += "... ";
                        _output.Write(status);

                        // device mode
                        if (isDelete)
                        {
                            if (!targetProfile.DeleteDevice(sourceDevice))
                            {
                                _output.WriteLine("device does not exist in this profile.");
                            }
                        }
                        else
                        {
                            if (!targetProfile.ReplaceDevice(sourceDevice))
                            {
                                _output.WriteLine("device does not exist in this profile.");
                            }
                        }

                        done = true;
                    }

                    if (done)
                    {
                        targetProfile.Save();
                        _output.WriteLine("Profile saved.");
                    }
                }
            }
            catch (Exception ex)
            {
                _output.WriteLine($"@Red{{{ex.Message}}}");
            }
        }

        private DeviceOperation(DeviceOperationType type, string sourceProfileName, string targetPath, string deviceId, string[] customEventIds, IOutput output)
        {
            Type = type;
            SourceProfileName = sourceProfileName;
            TargetPath = targetPath;
            DeviceId = deviceId;
            EventIds = customEventIds;
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
