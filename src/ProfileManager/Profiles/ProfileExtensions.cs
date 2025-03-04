using System.Xml;
using ProfileManager.Devices;

namespace ProfileManager.Profiles
{
    public static class ProfileExtensions
    {
        public static Device GetDevice(this Profile profile, string vendorID, string productID, int deviceIndex, int version)
        {
            vendorID = vendorID.ToLower();
            productID = productID.ToLower();

            return profile.Devices.FirstOrDefault(
                d => d.VendorID.ToLower() == vendorID 
                && d.ProductID.ToLower() == productID
                && (deviceIndex == -1 || d.DeviceIndex == deviceIndex) && (version == -1 || d.Version == version)
                );
        }

        public static IEnumerable<T> Project<T>(this XmlNode node, string name, Func<XmlNode, T> builder)
        {
            string xpath = $".//ns:{name}";
            return node.SelectNodes(xpath, Profile.NS).Cast<XmlNode>().Select(x => builder(x));
        }

        public static string EnsureXmlExtension(this string input)
        {
            if (input.EndsWith(".xml")) return input;
            return $"{input}.xml";
        }
    }
}
