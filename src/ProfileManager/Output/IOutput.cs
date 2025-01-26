namespace ProfileManager.Output
{
    public interface IOutput
    {
        string WaitPrompt { get; }
        Action<string> Write { get; }
        Action<string> WriteLine { get; }
        Action NewLine { get; }
        Action WaitForUser { get; }
    }
}
