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
        private IDictionary<string, (string VendorId, string ProductId, int DeviceIndex, int Version)> _nickNames;

        public string Path => _path;
        public string Name => System.IO.Path.GetFileNameWithoutExtension(_path);

        public IEnumerable<Device> Devices => _devices;

        public IEnumerable<DeviceComparison> CompareWith(Profile profile)
        {
            foreach(Device device in _devices)
            {
                Device otherDevice = profile.Devices.FirstOrDefault(d => d.VendorID == device.VendorID && d.ProductID == device.ProductID && d.DeviceIndex == device.DeviceIndex);
                DeviceComparison comparison = new DeviceComparison(device, otherDevice);

                yield return comparison;
            }
        }

        public bool ReplaceDevice(Device replacement)
        {
            string xpath = $"//ns:Device[@VendorID='{replacement.VendorID}' and @ProductID='{replacement.ProductID}' and @DeviceIndex='{replacement.DeviceIndex}' and @Version='{replacement.Version}']";
            XmlNode node = _profile.SelectSingleNode(xpath, _ns);
            if (node == null)
            {
                XmlNode newNode = replacement.Node.CloneNode(true);
                newNode = _profile.ImportNode(newNode, true);
                _profile.DocumentElement.ChildNodes[0].ChildNodes[0].AppendChild(newNode);
                _devices.Add(new Device(newNode));
                MapNicknames();
            }
            else
            {
                node.InnerXml = replacement.InnerXml;
            }
            
            return true;
        }
        public bool DeleteDevice(Device device)
        {
            string xpath = $"//ns:Device[@VendorID='{device.VendorID}' and @ProductID='{device.ProductID}' and @DeviceIndex='{device.DeviceIndex}' and @Version='{device.Version}']";
            XmlNode node = _profile.SelectSingleNode(xpath, _ns);
            if (node == null) return false;
            node.ParentNode.RemoveChild(node);
            _devices.Remove(device);
            MapNicknames();
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

        public void Backup()
        {
            string path = _path;
            string root = System.IO.Path.GetDirectoryName(path);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            path = System.IO.Path.Combine(root, name + ".bak");
            
            _profile.Save(path);
        }

        private void MapNicknames()
        {
            foreach (var nickname in _nickNames.Keys)
            {
                var deviceId = _nickNames[nickname];
                Device device = this.GetDevice(deviceId.VendorId, deviceId.ProductId, deviceId.DeviceIndex, deviceId.Version);
                if (device is not null)
                {
                    device.Nickname = nickname;
                }
            }
        }

        public Profile(string profilePath, string nicknamesFile)
        {
            if (nicknamesFile != null && File.Exists(nicknamesFile))
            {
                _nickNames = new Dictionary<string, (string, string, int, int)>();
                string[] lines = File.ReadAllLines(nicknamesFile);
                foreach (string line in lines)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        string nickname = parts[0];
                        string[] deviceParts = parts[1].Split(',');
                        if (deviceParts.Length > 1)
                        {
                            string vendorID = deviceParts[0];
                            string productID = deviceParts[1];
                            int deviceIndex = deviceParts.Length == 3 ? int.Parse(deviceParts[2]) : -1;
                            int version = deviceParts.Length == 4 ? int.Parse(deviceParts[3]) : -1;

                            _nickNames.Add(nickname, (vendorID, productID, deviceIndex, version));
                        }
                    }
                }
            }

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

            MapNicknames();
        }
    }
}
