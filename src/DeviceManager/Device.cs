using System.Xml;

namespace DeviceManager
{
    public class Device
    {
        private XmlNode _deviceNode;

        public string VendorID { get; set; }
        public string ProductID { get; set; }
        public int DeviceIndex { get; set; }
        public int Version { get; set; }
        public string Nickname { get; set; }

        public string Xml => _deviceNode.OuterXml;
        public string InnerXml => _deviceNode.InnerXml;
        public XmlNode Node => _deviceNode;

        public override string ToString()
        {
            return Nickname ?? $"{VendorID}:{ProductID}:{DeviceIndex}:{Version}";
        }

        public Device(XmlNode deviceNode)
        {
            _deviceNode = deviceNode;
            VendorID = deviceNode.Attributes["VendorID"].Value;
            ProductID = deviceNode.Attributes["ProductID"].Value;
            DeviceIndex = int.Parse(deviceNode.Attributes["DeviceIndex"].Value);
            Version = int.Parse(deviceNode.Attributes["Version"].Value);
        }
    }
}
