using ProfileManager.Devices;
using ProfileManager.Output;

namespace ProfileManager.Commands
{
    public class Delete : Command
    {
        private bool _makeBackups;

        public override bool RequiresArguments => true;
        public override string UsageText =>
@"@Green{Usage: ProfileManager delete [target] --device|-d:<nickname>|<vendorId,productId,deviceIndex,version>
| --event|-e:<boundTo>}
    target                 The profile to delete the device from (or all if not specified).
    --device|-d:<device>   The device to delete.
                           @Yellow{Specify either by device nickname (listed in nicknames.txt)}
                           @Yellow{or by a comma-delimited string giving vendorId, productId, 
                           deviceIndex and version}.
    --event|-e:<boundTo>   Delete events bound to <boundTo> @Yellow{(comma-delimited list: E_VS,BUTTON_1 etc)}.";

        public override void Execute()
        {
            string target = null;
            // if we have no profile specified, argument 0 should be the device name
            if (Arguments[0].IsValueOnly)
            {
                target = Arguments[0];
                target = GetFullPath(target, out bool isFolder, out bool pathIsValid);
                if (!pathIsValid)
                {
                    _output.WriteLine("@Red{Target profile or folder does not exist.}");
                    return;
                }
            }
            else
            {
                target = Directory.GetCurrentDirectory();
            }

            string device = Arguments[("device", "d")]?.Value;
            string[] events = Arguments[("event", "e")]?.GetValues();

            // check params
            if (string.IsNullOrWhiteSpace(device))
            {
                _output.WriteLine("@Red{Device is required.}");
                return;
            }

            DeviceOperation.Delete(target, device, events, _output, _makeBackups);
        }

        public Delete(ArgumentCollection arguments, IOutput output, bool makeBackups) : base(arguments, output)
        {
            _makeBackups = makeBackups;
        }
    }
}