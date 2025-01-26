using ProfileManager.Devices;
using ProfileManager.Output;

namespace ProfileManager.Commands
{
    public class Replace : Command
    {
        private bool _makeBackups;

        public override bool TakesArguments => true;
        public override string UsageText =>
@"@Green{Usage: ProfileManager replace source [target] --device|-d:<nickname>|<vendorId,productId[,deviceIndex][,version] | --event|-e:<boundTo>}
    source                  The profile to replace from.
    target                  The profile to replace into (or all if not specified).
    --device|-d:<device>    The device to replace.
                            @Yellow{Specify either by device nickname (listed in nicknames.txt)}
                            @Yellow{or by a comma-delimited string giving vendorId and productId}
                            @Yellow{(and optionally device index and version)}.
    OR
    --event:-e:<boundTo>    Replace just event bound to <boundTo>.";

        public override void Execute()
        {
            Argument argument = _arguments.FirstOrDefault();
            string source = null;
            string target = null;
            if (argument?.Value is null)
            { 
                source = GetFullPath(argument?.Value, out bool isFolder);
                if (argument?.Next?.Value is null)
                {
                    target = GetFullPath(argument.Next?.Value, out isFolder);
                }
            }

            string device = GetArgument("device", "d")?.Value;
            string @event = GetArgument("event", "e")?.Value;

            // check params
            if (string.IsNullOrWhiteSpace(source))
            {
                _output.WriteLine("@Red{Source profile is required.}");
                return;
            }
            if (string.IsNullOrWhiteSpace(device) && string.IsNullOrWhiteSpace(@event))
            {
                _output.WriteLine("@Red{Device or event is required.}");
                return;
            }

            DeviceReplacer replacer = new DeviceReplacer(source, target, device, @event, _output);
            replacer.Replace(this, _makeBackups);
        }

        public Replace(IEnumerable<Argument> arguments, IOutput output, bool makeBackups) : base(arguments, output)
        {
            _makeBackups = makeBackups;
        }
    }
}