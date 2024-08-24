namespace ProfileManager
{

    public class DeviceComparison
    {
        public string DeviceID { get; set; }
        public Device OriginalDevice { get; set; }
        public Device CompareDevice { get; set; }

        public bool IsEqual => OriginalDevice?.Xml == CompareDevice?.Xml;
        public bool DoesntExist => CompareDevice == null;

        public DeviceComparison(Device originalDevice, Device compareDevice)
        {
            OriginalDevice = originalDevice;
            CompareDevice = compareDevice;
            DeviceID = OriginalDevice?.ToString();
        }
    }
}
