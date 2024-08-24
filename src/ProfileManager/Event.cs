using System.Xml;

namespace ProfileManager
{
    public class Event
    {
        private XmlNode _eventNode;

        public IEnumerable<EventDefinition> EventDefinitions { get; set; }

        public string BoundTo { get; set; }

        public XmlNode Node => _eventNode;

        public Event(XmlNode eventNode)
        {
            _eventNode = eventNode;
            BoundTo = eventNode.Attributes["BoundTo"].Value;

            EventDefinitions = eventNode.Project("EventDefinition", x => new EventDefinition(x));
        }
    }
}
