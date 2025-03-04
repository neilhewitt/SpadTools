using ProfileManager.Devices;
using ProfileManager.Output;
using ProfileManager.Profiles;

namespace ProfileManager.Commands
{
    public class List : Command
    {
        public override string UsageText =>
@"@Green{Usage: ProfileManager list <profiles|devices>}
    <profiles|devices>  List profiles [profile folder path] | list devices [profile folder path].";

        public override string DescriptionText =>
@"@Yellow{List profiles} shows a list of Spad.neXt profiles in the current folder or the specified path. 
Profiles are any .xml files which use the Spad.neXt DTD.

@Yellow{List devices} shows a list of devices contained in the current folder or the specified path. 
Devices will be listed using the default ID format (vendorId, deviceId, deviceIndex (if not 0), 
version (if applicable), and the device nickname (nicknames are defined in a nicknames.txt file in the same
folder as the executable) if one exists.";

        public override bool RequiresArguments => true;

        public override void Execute()
        {
            if (HasArguments)
            {
                string path = Arguments.Count > 1 ? Arguments[1].Value : Directory.GetCurrentDirectory();

                switch (Arguments[0].Value)
                {
                    case "profiles":
                        ListProfiles(path);
                        break;
                    case "devices":
                        ListDevices(path);
                        break;
                    default:
                        ShowUsage();
                        break;
                }
            }
        }

        private void ListProfiles(string path)
        {
            if (!Directory.Exists(path))
            {
                _output.WriteLine($"@Red{{Invalid path: {path}}}");
                return;
            }

            IEnumerable<Profile> profiles = Profile.GetProfiles(path);
            if (profiles.Count() == 0)
            {
                _output.WriteLine($"No profiles found in {path}");
                return;
            }

            _output.WriteLine($"@Red{{{profiles.Count()} profiles}} found in @Green{{{path}}}:");

            int index = 1;
            foreach (Profile profile in profiles)
            {
                _output.WriteLine($"{index++} {profile.Name}: @Red{{{profile.Devices.Count()} devices}}, @Yellow{{{profile.CustomClientEvents.Count()} client events}}");
            }
        }

        private void ListDevices(string path)
        {
            if (!Directory.Exists(path))
            {
                _output.WriteLine($"@Red{{Invalid path: {path}}}");
                return;
            }

            IEnumerable<Profile> profiles = Profile.GetProfiles(path);
            List<Device> devices = new(profiles.SelectMany(p => p.Devices).Distinct());

            _output.WriteLine($"@Red{{{devices.Count()} devices}} found in @Red{{{profiles.Count()} profiles}} in @Green{{{path}}}:");

            int index = 1;
            foreach (Device device in devices)
            {
                _output.WriteLine($"{index++} {device.ToString(false)} : @Gray{{{(device.Nickname ?? "N/A")}}}");
            }
        }

        public List(ArgumentCollection arguments, IOutput output)
            : base(arguments, output)
        {
        }
    }
}
