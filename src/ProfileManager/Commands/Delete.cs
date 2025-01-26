using ProfileManager.Devices;
using ProfileManager.Output;

namespace ProfileManager.Commands
{
    public class Delete : Command
    {
        private bool _makeBackups;

        public override bool TakesArguments => true;
        public override string UsageText =>
@"@Green{Usage: ProfileManager delete [target] --device|-d:<nickname>|<vendorId,productId[,deviceIndex][,version] | --event|-e:<boundTo>}
    target                 The profile to delete the device from (or all if not specified).
    --device|-d:<device>   The device to delete.
                           @Yellow{Specify either by device nickname (listed in nicknames.txt)}
                           @Yellow{or by a comma-delimited string giving vendorId and productId}
                           @Yellow{(and optionally device index and version)}.
    OR
    --event:-e:<boundTo>   Delete custom client event bound to <boundTo>.";

        public override void Execute()
        {
            Argument argument = _arguments.FirstOrDefault();
            string target = null;
            if (argument?.Value is null)
            {
                target = GetFullPath(argument?.Value, out bool isFolder);
            }

            string device = GetArgument("device", "d")?.Value;
            string @event = GetArgument("event", "e")?.Value;

            // check params
            if (string.IsNullOrWhiteSpace(device) && string.IsNullOrWhiteSpace(@event))
            {
                _output.WriteLine("@Red{Device or event is required.}");
                return;
            }

            DeviceReplacer replacer = new DeviceReplacer(null, target, device, @event, _output);
            replacer.Delete(this, _makeBackups);

        }

        public Delete(IEnumerable<Argument> arguments, IOutput output, bool makeBackups) : base(arguments, output)
        {
            _makeBackups = makeBackups;
        }
    }
}