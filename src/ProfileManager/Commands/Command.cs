using ProfileManager.Output;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfileManager.Commands
{

    public abstract class Command
    {
        public static string DEFAULT_NICKNAMES_FILENAME = "nicknames.txt";

        protected IOutput _output;

        public ArgumentCollection Arguments { get; init; }
        public string Name => this.GetType().Name.ToLower();
        public virtual string UsageText => "";
        public virtual string DescriptionText => "";
        public virtual bool RequiresArguments => false;
        public bool HasArguments => Arguments?.Count() > 0;

        public abstract void Execute();

        public virtual void ShowUsage()
        {
            _output.WriteLine(UsageText);
        }

        public virtual void ShowDescription()
        {
            _output.WriteLine(DescriptionText);
        }

        protected string GetFullPath(string profileNameOrPath, out bool isFolder)
        {
            return GetFullPath(profileNameOrPath, out isFolder, out _, out _);
        }

        protected string GetFullPath(string profileNameOrPath, out bool isFolder, out string originalPath)
        {
            return GetFullPath(profileNameOrPath, out isFolder, out originalPath, out _);
        }

        protected string GetFullPath(string profileNameOrPath, out bool isFolder, out bool pathIsValid)
        {
            return GetFullPath(profileNameOrPath, out isFolder, out _, out pathIsValid);
        }

        protected string GetFullPath(string profileNameOrPath, out bool isFolder, out string originalPath, out bool pathIsValid)
        {
            originalPath = profileNameOrPath;
            isFolder = false;
            pathIsValid = true;

            string profileNameWithExtension = profileNameOrPath.EndsWith(".xml") ? profileNameOrPath : $"{profileNameOrPath}.xml";

            bool existsFile = File.Exists(profileNameWithExtension);
            bool existsFolder = Directory.Exists(profileNameOrPath);

            // is this already a valid full path?
            if (existsFolder)
            {
                isFolder = true;
                return profileNameOrPath.ToLower();
            }
            else if (existsFile)
            {
                return Path.GetFullPath(profileNameWithExtension).ToLower();
            }
            else
            {
                pathIsValid = false;
                return originalPath;
            }
        }

        protected virtual void EnsureArguments()
        {
            if (RequiresArguments && Arguments.Count() < 1)
            {
                ShowUsage();
            }
        }

        public Command(ArgumentCollection arguments, IOutput output)
        {
            _output = output;
            Arguments = arguments ?? new ArgumentCollection();
            if (Arguments.Count > 0) Arguments.RemoveAt(0); // remove the command name
            EnsureArguments();
        }
    }
}