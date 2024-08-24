using System.Xml;

namespace ProfileManager
{
    public class CustomClientEvent
    {
        private XmlNode _eventNode;

        public string Name { get; set; }
        public string EventID { get; set; }
        public string SubCategory { get; set; }
        public string Xml => this._eventNode.OuterXml;
        public string InnerXml => this._eventNode.InnerXml;

        public CustomClientEvent(XmlNode eventNode)
        {
            _eventNode = eventNode;
            Name = eventNode.Attributes["Name"].Value;
            EventID = eventNode.Attributes["EventId"]?.Value ?? eventNode.Attributes["EventID"].Value;
            SubCategory = eventNode.Attributes["SubCategory"].Value;
        }
    }
}
