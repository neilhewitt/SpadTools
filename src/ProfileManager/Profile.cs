using System.Xml;

namespace ProfileManager
{
    public class Profile
    {
        public static XmlNamespaceManager NS { get; set; }

        private string _path;
        private XmlDocument _profile;
        private IList<Device> _devices;
        private IList<CustomClientEvent> _customClientEvents;
        private IDictionary<string, (string VendorId, string ProductId, int DeviceIndex, int Version)> _nickNames = new Dictionary<string, (string, string, int, int)>();

        public string Path => _path;
        public string Name => System.IO.Path.GetFileNameWithoutExtension(_path);

        public IEnumerable<Device> Devices => _devices;

        public XmlDocument Document => _profile;

        public Device GetDevice(string nickname)
        {
            return Devices.FirstOrDefault(d => d.Nickname is not null && d.Nickname.ToLower() == nickname.ToLower());
        }

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
            XmlNode node = _profile.SelectSingleNode(xpath, NS);
            if (node == null)
            {
                XmlNode newNode = replacement.Node.CloneNode(true);
                newNode = _profile.ImportNode(newNode, true);
                
                xpath = "//ns:Devices";
                XmlNode devicesNode = _profile.SelectSingleNode(xpath, NS);

                devicesNode.AppendChild(newNode);
                _devices.Add(new Device(newNode, this));
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
            XmlNode node = _profile.SelectSingleNode(xpath, NS);
            if (node == null) return false;
            node.ParentNode.RemoveChild(node);
            _devices.Remove(device);
            MapNicknames();
            return true;
        }

        public bool ReplaceCustomClientEvent(CustomClientEvent replacement)
        {
            string xpath = $"//ns:CustomClientEvent[@Name='{replacement.Name}' and @EventID='{replacement.EventID}' and @SubCategory='{replacement.SubCategory}']";
            XmlNode node = _profile.SelectSingleNode(xpath, NS);
            if (node == null) return false;
            node.InnerXml = replacement.InnerXml;
            
            return true;
        }

        public bool ReplaceAllCustomClientEvents(Profile source)
        {
            string xpath = "//ns:CustomClientEvents";
            XmlNode sourceNode = source._profile.SelectSingleNode(xpath, NS);
            XmlNode thisNode = _profile.SelectSingleNode(xpath, NS);
            if (thisNode == null)
            {
                thisNode = (XmlNode)_profile.CreateElement("CustomClientEvents", NS.LookupNamespace("ns"));
                _profile.DocumentElement.AppendChild(thisNode);
            }
            thisNode.InnerXml = sourceNode.InnerXml;
            return true;
        }

        public bool DeleteAllCustomClientEvents()
        {
            XmlNode node = _profile.SelectSingleNode("//ns:CustomClientEvents", NS);
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
            if (nicknamesFile != null)
            {
                string nickLocal = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(profilePath), nicknamesFile);
                if (File.Exists(nickLocal))
                {
                    nicknamesFile = nickLocal;
                }
                else
                {
                    nicknamesFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), nicknamesFile);
                }

                if (File.Exists(nicknamesFile))
                {
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
            }

            if (!System.IO.Path.HasExtension(profilePath))
            {
                profilePath += ".xml";
            }

            _profile = new XmlDocument();
            _profile.Load(profilePath);

            // have to go through a complicated process to get the correctly-cased path
            // since this will be used to derive the profile name which is used in output a lot
            string directory = System.IO.Path.GetDirectoryName(profilePath);
            if (directory == "") directory = Directory.GetCurrentDirectory();
            profilePath = System.IO.Path.GetFullPath(profilePath);
            _path = Directory.EnumerateFiles(directory).FirstOrDefault(x => x.ToLower() == profilePath.ToLower());

            NS = new XmlNamespaceManager(_profile.NameTable);
            NS.AddNamespace("ns", "http://www.fsgs.com/SPAD");

            _devices = new List<Device>();
            XmlNodeList deviceNodes = _profile.SelectNodes("//ns:Device", NS);
            for (int i = 0; i < deviceNodes.Count; i++)
            {
                _devices.Add(new Device(deviceNodes[i], this));
            }

            _customClientEvents = new List<CustomClientEvent>();
            XmlNodeList customClientEventNodes = _profile.SelectNodes("//ns:CustomClientEvent", NS);
            for (int i = 0; i < customClientEventNodes.Count; i++)
            {
                _customClientEvents.Add(new CustomClientEvent(customClientEventNodes[i]));
            }

            MapNicknames();
        }
    }
}
