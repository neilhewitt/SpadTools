using ProfileManager.Devices;
using ProfileManager.Profiles;

namespace ProfileManager.Comparers
{
    public class DevicePageComparison
    {
        public Guid PageID { get; set; }
        public DevicePage Source { get; set; }
        public DevicePage Target { get; set; }

        public IEnumerable<EventComparison> Events { get; set; }

        public bool IsEqual => Source?.Node.OuterXml == Target?.Node.OuterXml;
        public bool DoesntExist => Target == null;
        public ComparisonResult Result => IsEqual ? ComparisonResult.Same : DoesntExist ? ComparisonResult.NotPresent : ComparisonResult.Different;
       
        public DevicePageComparison(DevicePage source, DevicePage target)
        {
            Source = source;
            Target = target;
            PageID = Source.ID;

            Events = Source.Events.Select(sourceEvent =>
            {
                Event targetEvent = Target?.Events.FirstOrDefault(x => x.BoundTo == sourceEvent.BoundTo);
                return new EventComparison(sourceEvent, targetEvent);
            }).ToList();
        }
    }
}
