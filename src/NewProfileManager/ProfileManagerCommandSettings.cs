using Spectre.Console.Cli;

namespace NewProfileManager
{
    public class ProfileManagerCommandSettings : CommandSettings
    {
        [CommandOption("--nobackups")]
        public virtual bool NoBackups { get; set; } = false;

        [CommandArgument(0, "[command]")]
        public virtual string Command { get; set; }
    }
}
