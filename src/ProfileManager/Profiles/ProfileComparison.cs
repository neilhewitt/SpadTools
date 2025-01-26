using ProfileManager.Devices;

namespace ProfileManager.Profiles
{
    public class ProfileComparison
    {
        public Profile SourceProfile { get; set; }
        public Profile TargetProfile { get; set; }

        public IEnumerable<DeviceComparison> DeviceComparisons { get; set; }

        public ProfileComparison(Profile sourceProfile, Profile targetProfile, string deviceId)
        {
            SourceProfile = sourceProfile;
            TargetProfile = targetProfile;

            if (deviceId is null)
            {
                DeviceComparisons = sourceProfile.CompareWith(targetProfile);
            }
            else
            {
                Device device = sourceProfile.GetDevice(deviceId);
                DeviceComparisons = sourceProfile.CompareWith(targetProfile, device);
            }
        }
    }
}
