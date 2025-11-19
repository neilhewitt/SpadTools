using ProfileManager.Devices;
using ProfileManager.Profiles;

namespace ProfileManager.Comparers
{
    public class ProfileComparison
    {
        public Profile SourceProfile { get; set; }
        public Profile TargetProfile { get; set; }
        public string DeviceId { get; set; }

        public IEnumerable<DeviceComparison> DeviceComparisons { get; set; }

        public DeviceComparison SpecifiedDeviceComparison => DeviceId is not null ? DeviceComparisons.FirstOrDefault(c => c.DeviceID.ToLower()== DeviceId.ToLower()) : null;

        public ProfileComparison(Profile sourceProfile, Profile targetProfile, string deviceId)
        {
            deviceId = deviceId?.ToLower();
            SourceProfile = sourceProfile;
            TargetProfile = targetProfile;

            List<DeviceComparison> deviceComparisons = new List<DeviceComparison>();
            foreach (Device sourceDevice in sourceProfile.Devices)
            {
                if (deviceId is null || sourceDevice.ID == deviceId || sourceDevice.Nickname.ToLower() == deviceId)
                {
                    Device targetDevice = TargetProfile.Devices.FirstOrDefault(d =>
                        d.VendorID == sourceDevice.VendorID &&
                        d.ProductID == sourceDevice.ProductID &&
                        d.DeviceIndex == sourceDevice.DeviceIndex &&
                        d.Version == sourceDevice.Version);

                    if (targetDevice is not null)
                    {
                        DeviceComparison comparison = new DeviceComparison(sourceDevice, targetDevice);
                        deviceComparisons.Add(comparison);
                    }
                }
            }

            DeviceComparisons = deviceComparisons;
            DeviceId = deviceId;
        }
    }
}
