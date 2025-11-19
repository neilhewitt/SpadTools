using System.Xml;
using SpadTools.Profiles.Events;

namespace SpadTools.Profiles.Devices
{
    public class DevicePage
    {
        private XmlNode _devicePageNode;

        public IEnumerable<Event> Events { get; set; }

        public Guid ID { get; set; }
        public string Name { get; set; }
        public bool IsDefaultPage { get; set; }

        public DevicePage(XmlNode devicePageNode)
        {
            _devicePageNode = devicePageNode;
            ID = Guid.Parse(devicePageNode.Attributes["ID"].Value);
            Name = devicePageNode.Attributes["Name"].Value;
            IsDefaultPage = bool.Parse(devicePageNode.Attributes["IsDefaultPage"].Value);

            Events = devicePageNode.Project("Event", x => new Event(x));
        }
    }
}
