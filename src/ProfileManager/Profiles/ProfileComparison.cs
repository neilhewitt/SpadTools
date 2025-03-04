using ProfileManager.Devices;

namespace ProfileManager.Profiles
{
    public class ProfileComparison
    {
        public Profile SourceProfile { get; set; }
        public Profile TargetProfile { get; set; }

        public IEnumerable<DeviceComparison> DeviceComparisons { get; set; }

        public ProfileComparison(Profile sourceProfile, Profile targetProfile, IEnumerable<DeviceComparison> deviceComparisons)
        {
            SourceProfile = sourceProfile;
            TargetProfile = targetProfile;
            DeviceComparisons = deviceComparisons;
        }
    }
}
