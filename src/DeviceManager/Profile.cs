using System.Xml;

namespace DeviceManager
{
    public class Profile
    {
        private string _path;
        private XmlDocument _profile;
        private XmlNamespaceManager _ns;
        private IList<Device> _devices;
        private IList<CustomClientEvent> _customClientEvents;

        public string Path => _path;

        public IEnumerable<Device> Devices => _devices;

        public bool ReplaceDevice(Device replacement)
        {
            string xpath = $"//ns:Device[@VendorID='{replacement.VendorID}' and @ProductID='{replacement.ProductID}' and @DeviceIndex='{replacement.DeviceIndex}' and @Version='{replacement.Version}']";
            XmlNode node = _profile.SelectSingleNode(xpath, _ns);
            if (node == null) return false;
            node.InnerXml = replacement.InnerXml;
            
            return true;
        }
        public bool DeleteDevice(Device device)
        {
            string xpath = $"//ns:Device[@VendorID='{device.VendorID}' and @ProductID='{device.ProductID}' and @DeviceIndex='{device.DeviceIndex}' and @Version='{device.Version}']";
            XmlNode node = _profile.SelectSingleNode(xpath, _ns);
            if (node == null) return false;
            node.ParentNode.RemoveChild(node);
            return true;
        }

        public bool ReplaceCustomClientEvent(CustomClientEvent replacement)
        {
            string xpath = $"//ns:CustomClientEvent[@Name='{replacement.Name}' and @EventID='{replacement.EventID}' and @SubCategory='{replacement.SubCategory}']";
            XmlNode node = _profile.SelectSingleNode(xpath, _ns);
            if (node == null) return false;
            node.InnerXml = replacement.InnerXml;
            
            return true;
        }

        public bool ReplaceAllCustomClientEvents(Profile source)
        {
            string xpath = "//ns:CustomClientEvents";
            XmlNode sourceNode = source._profile.SelectSingleNode(xpath, source._ns);
            XmlNode thisNode = _profile.SelectSingleNode(xpath, _ns);
            if (thisNode == null)
            {
                thisNode = (XmlNode)_profile.CreateElement("CustomClientEvents", _ns.LookupNamespace("ns"));
                _profile.DocumentElement.AppendChild(thisNode);
            }
            thisNode.InnerXml = sourceNode.InnerXml;
            return true;
        }

        public bool DeleteAllCustomClientEvents()
        {
            XmlNode node = _profile.SelectSingleNode("//ns:CustomClientEvents", _ns);
            if (node == null) return false;
            node.InnerXml = "";
            return true;
        }

        public void Save()
        {
            _profile.Save(_path);
        }

        public Profile(string profilePath)
        {
            // fix path casing
            if (File.Exists(profilePath))
            {
                string[] files = Directory.GetFiles(Directory.GetCurrentDirectory()).Select(x => System.IO.Path.GetFileName(x)).ToArray();
                profilePath = files.FirstOrDefault(x => x.ToLower() == profilePath.ToLower());
            }
            else
            {
                throw new FileNotFoundException("Profile not found", profilePath);
            }   

            _path = profilePath;
            _profile = new XmlDocument();
            _profile.Load(profilePath);

            _ns = new XmlNamespaceManager(_profile.NameTable);
            _ns.AddNamespace("ns", "http://www.fsgs.com/SPAD");

            _devices = new List<Device>();
            XmlNodeList deviceNodes = _profile.SelectNodes("//ns:Device", _ns);
            for (int i = 0; i < deviceNodes.Count; i++)
            {
                _devices.Add(new Device(deviceNodes[i]));
            }

            _customClientEvents = new List<CustomClientEvent>();
            XmlNodeList customClientEventNodes = _profile.SelectNodes("//ns:CustomClientEvent", _ns);
            for (int i = 0; i < customClientEventNodes.Count; i++)
            {
                _customClientEvents.Add(new CustomClientEvent(customClientEventNodes[i]));
            }
        }
    }
}
