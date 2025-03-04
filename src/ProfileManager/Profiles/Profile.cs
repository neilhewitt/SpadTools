using ProfileManager.Devices;
using System.Linq;
using System.Xml;

namespace ProfileManager.Profiles
{
    public class Profile
    {
        public static IEnumerable<Profile> GetProfiles(string pathOrProfile, string sourceProfile = null)
        {
            pathOrProfile = pathOrProfile.ToLower();
            sourceProfile = sourceProfile?.EnsureXmlExtension().ToLower();

            // is this a single profile XML file?
            if ((pathOrProfile.EndsWith(".xml") && File.Exists(pathOrProfile)) || (File.Exists(pathOrProfile + ".xml")))
            {
                yield return new Profile(pathOrProfile, Profile.DEFAULT_NICKNAMES_FILENAME);
                yield break;
            }

            if (pathOrProfile is null)
            {
                Console.WriteLine("FRAK: No path or profile specified.");
                pathOrProfile = Directory.GetCurrentDirectory();
            }

            if (!Directory.Exists(pathOrProfile))
            {
                throw new FileNotFoundException($"Directory not found: {pathOrProfile}");
            }

            foreach (var profilePath in Directory.GetFiles(pathOrProfile).Select(x => x.ToLower()).Where(x => x.EndsWith(".xml")))
            {
                Profile profile = null;
                try
                {
                    // don't want to include the source profile
                    if (profilePath != sourceProfile)
                    {
                        profile = new(profilePath, Profile.DEFAULT_NICKNAMES_FILENAME);
                    }
                }
                catch
                {
                    // not valid for whatever reason
                    // do nothing, we'll just skip this file
                }

                if (profile is not null)
                {
                    yield return profile;
                }
            }
        }

        public static Profile GetProfile(string profileName)
        {
            profileName = profileName.EnsureXmlExtension().ToLower();
            return new Profile(profileName, Profile.DEFAULT_NICKNAMES_FILENAME);
        }

        public static string DEFAULT_NICKNAMES_FILENAME = "nicknames.txt";
        public static XmlNamespaceManager NS { get; set; }

        private string _path;
        private XmlDocument _profile;
        private IList<Device> _devices;
        private IList<CustomClientEvent> _customClientEvents;
        private IDictionary<string, (string VendorId, string ProductId, int DeviceIndex, int Version)> _nickNames = new Dictionary<string, (string, string, int, int)>();

        public string Path => _path;
        public string Name => System.IO.Path.GetFileNameWithoutExtension(_path);

        public IEnumerable<Device> Devices => _devices;
        public IEnumerable<CustomClientEvent> CustomClientEvents => _customClientEvents;

        public XmlDocument Document => _profile;

        public Device GetDevice(string id)
        {
            Device device = Devices.FirstOrDefault(d => d.Nickname is not null && d.Nickname.ToLower() == id.ToLower());
            if (device is null)
            {
                try
                {
                    string[] idParts = id.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    if (idParts.Length == 4)
                    {
                        string vendorID = idParts[0];
                        string productID = idParts[1];
                        int deviceIndex = int.Parse(idParts[2]);
                        int version = int.Parse(idParts[3]);
                        device = Devices.SingleOrDefault(d => d.VendorID == vendorID && d.ProductID == productID && d.DeviceIndex == deviceIndex && d.Version == version);
                        return device;
                    }
                    else
                    {
                        throw new FormatException("Invalid nickname, or device ID format: must be 'VendorID,ProductID,DeviceIndex,Version' and VendorId and ProductID must be in hex format '0xXXXX'.");
                    }
                }
                catch (FormatException)
                {
                    throw;
                }
                catch
                {
                    throw;
                }
            }

            return device;
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
                node.Attributes["DevicePath"].Value = replacement.DevicePath;
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

        private Profile(string profilePath, string nicknamesFile)
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

            if (!_profile.DocumentElement.OuterXml.Contains("xmlns=\"http://www.fsgs.com/SPAD\""))
            {
               throw new FormatException("Not a Spad.neXt profile.");
            }

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
