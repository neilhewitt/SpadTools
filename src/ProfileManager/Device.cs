using System.Xml;
using System.Linq;
using System.Net;

namespace ProfileManager
{
    public class Device
    {
        private Profile _profile;
        private XmlNode _deviceNode;

        public IEnumerable<DevicePage> Pages { get; set; }

        public string VendorID { get; set; }
        public string ProductID { get; set; }
        public int DeviceIndex { get; set; }
        public int Version { get; set; }
        public string Nickname { get; set; }

        public string Xml => _deviceNode.OuterXml;
        public string InnerXml => _deviceNode.InnerXml;
        public XmlNode Node => _deviceNode;

        public Event GetEvent(string boundTo)
        {
            return Pages.SelectMany(p => p.Events).FirstOrDefault(e => e.BoundTo.ToLower() == boundTo.ToLower());
        }

        public bool ReplaceEvent(string boundTo, Event replacement)
        {
            // boundTo may need casing fixed
            boundTo = replacement.BoundTo;

            string xpath = $".//ns:Event[@BoundTo='{boundTo}']";
            XmlNode node = _deviceNode.SelectSingleNode(xpath, Profile.NS);
            if (node == null)
            {
                XmlNode newNode = replacement.Node.CloneNode(true);
                newNode.Attributes["BoundTo"].Value = boundTo;
                newNode = _profile.Document.ImportNode(newNode, true);
                _deviceNode.FirstChild.FirstChild.AppendChild(newNode);
                return false; // indicates the node was added, not replaced
            }
            else
            {
                node.InnerXml = replacement.Node.InnerXml;
            }

            return true;
        }

        public bool DeleteEvent(string boundTo)
        {
            string xpath = $".//ns:Event[@BoundTo='{boundTo}']";
            XmlNode node = _deviceNode.SelectSingleNode(xpath, Profile.NS);
            if (node == null) return false;
            node.ParentNode.RemoveChild(node);
            return true;
        }

        public override string ToString()
        {
            return Nickname ?? $"{VendorID}:{ProductID}:{DeviceIndex}:{Version}";
        }

        public Device(XmlNode deviceNode, Profile profile)
        {
            _profile = profile;
            _deviceNode = deviceNode;
            VendorID = deviceNode.Attributes["VendorID"].Value;
            ProductID = deviceNode.Attributes["ProductID"].Value;
            DeviceIndex = int.Parse(deviceNode.Attributes["DeviceIndex"].Value);
            Version = int.Parse(deviceNode.Attributes["Version"].Value);
            
            Pages = deviceNode.Project("DevicePage", x => new DevicePage(x));
        }
    }
}
