using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace NewProfileManager.Commands
{
    public class ListCommand : Command<ListCommand.ListSettings>
    {
        public static string HelpText =>
@"[yellow]List profiles[/yellow] shows a list of Spad.neXt profiles in the current folder or the specified path. 
Profiles are any .xml files which use the Spad.neXt DTD.

@[yellow]List devices[/yellow] shows a list of devices contained in the current folder or the specified path. 
Devices will be listed using the default ID format (vendorId, deviceId, deviceIndex (if not 0), 
version (if applicable), and the device nickname (nicknames are defined in a nicknames.txt file in the same
folder as the executable) if one exists.";

        public class ListSettings : ProfileManagerCommandSettings
        {
            [Description("Type of items to list. Use 'profiles' to list profiles or 'devices' to list devices.")]
            [CommandArgument(0, "<profiles|devices|help>")]
            public string ListType { get; set; }

            [Description("Path to the folder containing profiles or devices. Defaults to the current directory.")]
            [CommandArgument(1, "[path]")]
            public string Path { get; set; } = Environment.CurrentDirectory;
        }

        public override int Execute(CommandContext context, ListSettings settings)
        {
            string path = settings.Path;
            switch (settings.ListType.ToLowerInvariant())
            {
                case "profiles":
                    ListProfiles(path);
                    break;
                case "devices":
                    ListDevices(path);
                    break;
                case "help":
                    AnsiConsole.MarkupLine(HelpText);
                    break;
                default:
                    AnsiConsole.MarkupLine($"[red]Invalid type: {settings.ListType}[/]");
                    return -1;
            }
            return 0;
        }


        private void ListProfiles(string path)
        {
            if (!Directory.Exists(path))
            {
                AnsiConsole.WriteLine($"[red]Invalid path: {path}[/]");
                return;
            }

            //IEnumerable<Profile> profiles = Profile.GetProfiles(path);
            //if (profiles.Count() == 0)
            //{
            //    AnsiConsole.MarkupLine($"[red]No profiles found in {path}[/]");
            //    return;
            //}

            //AnsiConsole.MarkupLine($"[red]{profiles.Count()} profiles found in [green]{path}[/]:");

            //int index = 1;
            //foreach (Profile profile in profiles)
            //{
            //    AnsiConsole.MarkupLine($"{index++} {profile.Name}: [red]{profile.Devices.Count()} devices[/], [yellow]{profile.CustomClientEvents.Count()} client events[/]");
            //}
        }

        private void ListDevices(string path)
        {
            if (!Directory.Exists(path))
            {
                AnsiConsole.MarkupLine($"[red]Invalid path: {path}[/]");
                return;
            }

            //IEnumerable<Profile> profiles = Profile.GetProfiles(path);
            //List<Device> devices = new(profiles.SelectMany(p => p.Devices).Distinct());

            //AnsiConsole.MarkupLine($"[red]{devices.Count()} devices found in [green]{path}[/]:");

            //int index = 1;
            //foreach (Device device in devices)
            //{
            //    AnsiConsole.MarkupLine($"{index++} {device.ToString()} : @Gray{{{(device.Nickname ?? "N/A")}}}");
            //}
        }
    }
}
