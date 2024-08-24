using System.Xml;

namespace ProfileManager
{
    public class EventDefinition
    {
        private XmlNode _eventDefinitionNode;

        public string Trigger { get; set; }
        public string ActionXml => _eventDefinitionNode.InnerXml;

        public EventDefinition(XmlNode eventDefinitionNode)
        {
            _eventDefinitionNode = eventDefinitionNode;
            Trigger = eventDefinitionNode.Attributes["Trigger"].Value;
        }
    }
}
