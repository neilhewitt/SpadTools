namespace DeviceManager
{
    public interface IOutput
    {
        Action<string> Write { get; }
        Action<string> WriteLine { get; }
        Action NewLine { get; }
        Action WaitForUser { get; }
    }
}
