using ProfileManager.Devices;
using ProfileManager.Profiles;

namespace ProfileManager.Comparers
{
    public class DeviceComparison
    {
        public string DeviceID { get; set; }
        public Device Source { get; set; }
        public Device Target { get; set; }

        public IEnumerable<DevicePageComparison> Pages { get; set; }

        public bool IsEqual => Source?.Xml == Target?.Xml;
        public bool DoesntExist => Target == null;
        public ComparisonResult Result => IsEqual ? ComparisonResult.Same : DoesntExist ? ComparisonResult.NotPresent : ComparisonResult.Different;

        public DeviceComparison(Device source, Device target)
        {
            Source = source;
            Target = target;
            DeviceID = Source?.ToString();

            Pages = Source.Pages.Select(sourcePage =>
            {
                DevicePage targetPage = Target?.Pages.FirstOrDefault(x => x.ID == sourcePage.ID);
                return new DevicePageComparison(sourcePage, targetPage);
            }).ToList();
        }
    }
}
