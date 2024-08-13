namespace DeviceManager
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

        public static Device GetDevice(this Profile profile, string nickname)
        {
            return profile.Devices.FirstOrDefault(d => d.Nickname is not null && d.Nickname.ToLower() == nickname.ToLower());
        }
    }
}
