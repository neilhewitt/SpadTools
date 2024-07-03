namespace DeviceReplace
{
    internal class Program
    {
        static void Main(string[] args) 
        {
            bool noBackups = false;

            if (args[0] == "debug")
            {
                Console.WriteLine("Attach debugger and press ENTER");
                Console.ReadLine();

                args = args.Skip(1).ToArray();
            }
            
            if (args[0] == "--nobackups" || args[0] == "-nb")
            {
                Console.WriteLine("Backups disabled");
                noBackups = true;
                args = args.Skip(1).ToArray();
            }

            if (args.Length == 0 || args[0].Contains("help"))
            {
                Console.WriteLine("Usage: DeviceReplace [--nobackups|-nb] <fromProfile> <toProfile|*> <vendorId> <deviceId> [index] [version]");
                return;
            }

            try
            {
                if (args.Length < 4) throw new Exception("Not enough arguments");

                string from = args[0].ToLower();
                string to = args[1].ToLower();
                string vendorId = args[2].ToLower();
                string deviceId = args[3].ToLower();
                int? index = null;
                int? version = null;
                if (args.Length > 4)
                {
                    index = int.Parse(args[4]);
                    version = int.Parse(args[5]);
                }

                Profile fromProfile = new Profile(from);
                List<string> toProfiles = new List<string>();
                if (to == "*")
                {
                    toProfiles.AddRange(Directory.GetFiles(Environment.CurrentDirectory, "*.xml").Select(x => Path.GetFileName(x.ToLower())));
                    toProfiles.Remove(from);
                }
                else
                {
                    toProfiles.Add(to);
                }

                foreach (string profilePath in toProfiles)
                {
                    Profile toProfile = new Profile(profilePath);
                    Device fromDevice = fromProfile.Devices.FirstOrDefault(d => d.VendorID.ToLower() == vendorId && d.ProductID.ToLower() == deviceId
                                        && ((index is null && version is null) || (d.DeviceIndex == index && d.Version == version)));
                    if (fromDevice == null) throw new Exception("Device not found in source profile");

                    if (!noBackups)
                    {
                        string backupPath = Path.ChangeExtension(toProfile.Path, "bak");
                        File.Copy(toProfile.Path, backupPath, true);
                        Console.WriteLine($"Backup created: {backupPath}");
                    }

                    toProfile.ReplaceDevice(fromDevice);
                    toProfile.Save();
                    Console.WriteLine($"Device {vendorId}:{deviceId} replaced in {toProfile.Path}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Invalid arguments: " + ex.Message);
            }
        }
    }
}
