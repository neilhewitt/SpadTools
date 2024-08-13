namespace DeviceManager
{
    public static class CommandExtensions
    {
        public static bool Matches(this string argument, string argumentName, string shortArgumentName)
        {
            argument = argument.ToLower();
            argumentName = argumentName.ToLower();
            shortArgumentName = shortArgumentName.ToLower();
            bool found = false;
            
            found = argument.Contains($"--{argumentName}");
            if (!found) found = argument.Contains($"-{shortArgumentName} "); // avoid matching short versions that start with the same substring
            if (!found) found = argument.Contains($"-{shortArgumentName}:"); // argument with parameter
            if (!found) found = argument.EndsWith($"-{shortArgumentName}"); // trailing argument without parameter

            return found;
        }
    }
}
