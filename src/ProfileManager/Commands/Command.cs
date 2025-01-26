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

        public static string GetFullPath(string profileNameOrPath, out bool isFolder)
        {
            isFolder = false;

            if (string.IsNullOrWhiteSpace(profileNameOrPath))
            {
                return null;
            }

            if (profileNameOrPath.EndsWith("\\*"))
            {
                profileNameOrPath = profileNameOrPath.TrimEnd('\\', '*');
            }

            // is this already a valid full path?
            if (Directory.Exists(profileNameOrPath))
            {
                isFolder = true;
                return profileNameOrPath.ToLower();
            }

            return Path.GetFullPath(Path.ChangeExtension(profileNameOrPath, "xml")).ToLower();
        }

        public static IEnumerable<string> GetProfilePaths(string path = null)
        {
            if (path is null || path == "*")
            {
                path = Environment.CurrentDirectory;
            }

            if (path.EndsWith("\\*"))
            {
                path = path.TrimEnd('\\', '*');
            }

            if (File.Exists(path) || File.Exists(path + ".xml"))
            {
                return new[] { path };
            }

            return (Directory.GetFiles(path, "*.xml")).Select(x => x.ToLower());
        }

        protected List<Argument> _arguments;
        protected IOutput _output;

        public string Name => this.GetType().Name.ToLower();
        public virtual string UsageText => "";
        public virtual string DescriptionText => "";
        public virtual bool TakesArguments => false;
        public bool HasArguments => _arguments?.Count() > 0;

        public abstract void Execute();

        protected Argument GetArgument(string argumentName, string shortArgumentName)
        {
            return _arguments.FirstOrDefault(a => a.Matches(argumentName, shortArgumentName));
        }

        public virtual void ShowUsage()
        {
            _output.WriteLine(UsageText);
        }

        public virtual void ShowDescription()
        {
            _output.WriteLine(DescriptionText);
        }

        protected virtual void EnsureArguments()
        {
            if (TakesArguments && _arguments.Count() < 1)
            {
                ShowUsage();
            }
        }

        public Command(IEnumerable<Argument> arguments, IOutput output)
        {
            _output = output;
            _arguments = new List<Argument>((arguments?.Count() ?? 0) > 1 ? arguments.Skip(1) : new Argument[0]); // first argument is the command name
            if (arguments is not null)
            {
                EnsureArguments();
            }
        }
    }
}