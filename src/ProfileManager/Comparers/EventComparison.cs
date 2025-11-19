using ProfileManager.Profiles;

namespace ProfileManager.Comparers
{
    public class EventComparison
    {
        public string EventID { get; set; }
        public Event Source { get; set; }
        public Event Target { get; set; }
        public bool IsEqual => Source?.Node.OuterXml == Target?.Node.OuterXml;
        public bool DoesntExist => Target == null;
        public ComparisonResult Result => IsEqual ? ComparisonResult.Same : DoesntExist ? ComparisonResult.NotPresent : ComparisonResult.Different;
        public EventComparison(Event source, Event target)
        {
            Source = source;
            Target = target;
            EventID = Source?.BoundTo;
        }
    }
}
