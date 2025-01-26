using ProfileManager.Profiles;

namespace ProfileManager.Devices
{
    public class DeviceComparison
    {
        public string DeviceID { get; set; }
        public Device OriginalDevice { get; set; }
        public Device CompareDevice { get; set; }

        public bool IsEqual => OriginalDevice?.Xml == CompareDevice?.Xml;
        public bool DoesntExist => CompareDevice == null;
        public ComparisonResult Result => IsEqual ? ComparisonResult.Same : DoesntExist ? ComparisonResult.NotPresent : ComparisonResult.Different;

        public DeviceComparison(Device originalDevice, Device compareDevice)
        {
            OriginalDevice = originalDevice;
            CompareDevice = compareDevice;
            DeviceID = OriginalDevice?.ToString();
        }
    }
}
