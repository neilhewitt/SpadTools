using ProfileManager.Devices;
using ProfileManager.Output;

namespace ProfileManager.Commands
{
    public class Replace : Command
    {
        private bool _makeBackups;

        public override bool RequiresArguments => true;
        public override string UsageText =>
@"@Green{Usage: ProfileManager replace source [target] --device|-d:<nickname>|<vendorId,productId,deviceIndex,version>
| --event|-e:<boundTo>}
    source                  The profile to replace from.
    target                  The profile to replace into (or all if not specified).
    --device|-d:<device>    The device to replace.
                            @Yellow{Specify either by device nickname (listed in nicknames.txt)}
                            @Yellow{or by a comma-delimited string giving vendorId, productId, 
                            deviceIndex and version}.
    OR
    --event:-e:<boundTo>    Replace just event bound to <boundTo>.";

        public override void Execute()
        {
            string source = Arguments[0];
            string target = Arguments[1];

            if (string.IsNullOrWhiteSpace(source))
            {
                _output.WriteLine("@Red{Source profile is required.}");
                return;
            }

            source = GetFullPath(source, out bool isFolder, out bool pathIsValid);
            if (!pathIsValid)
            {
                _output.WriteLine("@Red{Source profile does not exist.}");
                return;
            }

            if (target is not null)
            {
                target = GetFullPath(target, out isFolder);
            }
            
            string device = Arguments[("device", "d")]?.Value;
            string @event = Arguments[("event", "e")]?.Value;

            if (string.IsNullOrWhiteSpace(device) && string.IsNullOrWhiteSpace(@event))
            {
                _output.WriteLine("@Red{Device or event is required.}");
                return;
            }

            DeviceOperation.Replace(source, target, device, @event, _output, _makeBackups);
        }

        public Replace(ArgumentCollection arguments, IOutput output, bool makeBackups) : base(arguments, output)
        {
            _makeBackups = makeBackups;
        }
    }
}