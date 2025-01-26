using ProfileManager.Output;
using ProfileManager.Profiles;

namespace ProfileManager.Commands
{
    public class Compare : Command
    {
        public override bool TakesArguments => true;

        public override string UsageText =>
@"@Green{Usage: ProfileManager compare <source> [<target>] [--device|-d:<device>] [--filter|-f:<same|different|notpresent>] [--csv|-c:<path>] [-nodisplay]}
    <source>                The profile to compare from.
    <target>                The profile to compare against (or all if not specified).
    --device|-d:<device>    The device to compare (if not specified, all devices are compared).
    --filter|-f:<filter>    Show only results of the specified type.
    --csv|-c:<path>         The path to write the CSV report to (if none, no CVS report is written).
    --nodisplay|-nd         Suppress output to the console.";

        public override string DescriptionText =>
@"@Yellow{Compares the specific content of one or more devices across two or more profiles.}

<source> is the source profile against which other profiles will be compared. This can be a full path to a
Spad.neXt profile XML file, or the name of a profile XML file in the current folder. You can omit the .xml
extension when specifying profiles.

<target> is the target profile against which the specified devices will be compared. If this is omitted
then the comparison will be run against all profiles (other than the source profile) in the folder
specified by the source path.

If you specify a device using the --device option, only this device will be compared. Otherwise all devices in all
profiles will be compared. Devices may be specified using their nickname (see the nicknames.txt file for details)
or using the standard format @Blue{0xVVVV,0xDDDD,I,V} where VVVV is the vendorID in hex, DDDD is the device ID in hex,
I is the index in decimal and V is the version in decimal.

The comparison will return one of three statuses for each device:
- Same: the devices are identical between these profiles
- Different: the devices are different between these profiles
- Not present: the device is not present in the target profile

You can filter to see just one of these statuses by using the --filter option.

If you specify the --csv flag with a valid output path, the results of the comparison will be written to a CSV file
in the same folder as the profiles.

If you specify the --nodisplay flag then the comparison results will not be written to the output, but the CSV (if requested)
will still be written.";

        public override void Execute()
        {
            Argument argument = _arguments.FirstOrDefault();
            string source = GetFullPath(argument?.Value, out bool isSourceAFolder); 
            string target = GetFullPath(argument.Next?.Value, out bool isTargetAFolder) ?? "*"; // wildcard if not specified

            if (isSourceAFolder)
            {
                _output.WriteLine("@Red{Source profile must be a file, not a folder.}");
                return;
            }

            if (isTargetAFolder)
            {
                target = target.TrimEnd('\\') + "\\*";
            }

            string device = GetArgument("device", "d")?.Value;
            string filter = GetArgument("filter", "f")?.Value;
            string csvPath = GetArgument("csv", "c")?.Value;
            bool noDisplay = GetArgument("nodisplay", "nd") != null;

            // check params
            if (string.IsNullOrWhiteSpace(source))
            {
                _output.WriteLine("@Red{Source profile is required.}");
                return;
            }

            ProfileComparer comparer = new ProfileComparer(source, target, device, filter, csvPath, !noDisplay, _output);
            comparer.Compare(this);
        }

        public Compare(IEnumerable<Argument> arguments, IOutput output) : base(arguments, output)
        {
        }
    }
}
