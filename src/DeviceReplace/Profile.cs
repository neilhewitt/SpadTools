using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DeviceReplace
{
    public class Profile
    {
        private string _path;
        private XmlDocument _profile;
        private XmlNamespaceManager _ns;
        private IList<Device> _devices;

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
        }   
    }
}
