namespace DeviceManager
{
    public class Program
    {
        private static string _prompt = "Usage: device replace|delete [--nobackups|-nb] [--customclientevents|-ce] <fromProfile|*> [<toProfile|*>] [<vendorId>] [<deviceId>] [index] [version]";

        private static void Main(string[] args)
        {
            bool noBackups = false;
            bool customClientEventMode = false;
            string empty = string.Empty;
            if (args.Length == 0 || args[0].Contains("help"))
            {
                Console.WriteLine(_prompt);
            }
            else
            {
                if (args[0] == "debug")
                {
                    Console.WriteLine("Attach debugger and press ENTER");
                    Console.ReadLine();
                    args = args.Skip(1).ToArray();
                }

                if (args[0] != "replace" && args[0] != "delete")
                {
                    Console.WriteLine("Invalid arguments. " + _prompt);
                }
                else
                {
                    string mode = args[0];
                    args = args.Skip(1).ToArray();

                    if (args[0] == "--nobackups" || args[0] == "-nb")
                    {
                        Console.WriteLine("Backups disabled");
                        noBackups = true;
                        args = args.Skip(1).ToArray();
                    }

                    if (args[0] == "--customclientevents" || args[0] == "-ce")
                    {
                        customClientEventMode = true;
                        args = args.Skip(1).ToArray();
                    }

                    try
                    {
                        if (!customClientEventMode && args.Length < 3) throw new Exception("Not enough arguments.");

                        bool hasTo = args.Length > 1;
                        if (hasTo)
                        {
                            string path = args[1].ToLower();
                            if (path != "*" && !Path.Exists(path))
                                hasTo = false;
                        }

                        int argIndex = 0;
                        string from = args[argIndex++].ToLower();
                        string to = hasTo ? args[argIndex++].ToLower() : null;
                        string vendorId = !customClientEventMode ? args[argIndex++].ToLower() : null;
                        string deviceId = !customClientEventMode ? args[argIndex++].ToLower() : null;
                        int? index = new int?();
                        int? version = new int?();
                        if (args.Length > argIndex && !customClientEventMode)
                        {
                            index = new int?(int.Parse(args[argIndex++]));
                            version = new int?(int.Parse(args[argIndex++]));
                        }

                        if (from == "*" && mode == "replace")
                            throw new Exception("To replace a device / client events you must specify a source profile");

                        if (mode == "replace" && to == null)
                            throw new Exception("No target profile specified");

                        Profile fromProfile = from != "*" ? new Profile(from) : null;
                        List<string> toProfiles = new List<string>();
                        if (to == "*" || from == "*")
                        {
                            toProfiles.AddRange((Directory.GetFiles(Environment.CurrentDirectory, "*.xml")).Select(x => Path.GetFileName(x.ToLower())));
                            if (from != "*")
                            {
                                toProfiles.Remove(from);
                            }
                        }
                        else if (to != null)
                        {
                            toProfiles.Add(to);
                        }
                        else
                        {
                            toProfiles.Add(from);
                        }

                        foreach (string toProfile in toProfiles)
                        {
                            Profile profile = new Profile(toProfile);

                            if (customClientEventMode)
                            {
                                if (mode == "replace")
                                {
                                    if (fromProfile == null) throw new Exception("No source profile specified for custom client events");

                                    profile.ReplaceAllCustomClientEvents(fromProfile);
                                }
                                else
                                {
                                    profile.DeleteAllCustomClientEvents();
                                }

                                profile.Save();
                                Console.WriteLine("Custom client events " + mode + "d in " + profile.Path);
                            }
                            else
                            {
                                (Device Device, string Path) deviceInfo = from != "*" ? getDevice(fromProfile) : getDevice(profile);
                                if (deviceInfo.Device == null)
                                {
                                    Console.WriteLine($"Device {deviceInfo.Device.VendorID}:{deviceInfo.Device.ProductID} not found in source profile {deviceInfo.Path}");
                                }
                                else
                                {
                                    if (!noBackups)
                                    {
                                        string destFileName = Path.ChangeExtension(profile.Path, "bak");
                                        File.Copy(profile.Path, destFileName, true);
                                        Console.WriteLine("Backup created: " + destFileName);
                                    }

                                    if (mode == "replace")
                                    {
                                        profile.ReplaceDevice(deviceInfo.Item1);
                                    }
                                    else
                                    {
                                        profile.DeleteDevice(deviceInfo.Item1);
                                    }
                                }

                                profile.Save();
                                Console.WriteLine($"Device {vendorId}:{deviceId} {mode}d in {profile.Path}");
                            }
                        }

                        (Device Device, string Path) getDevice(Profile profile)
                        {
                            return (profile.Devices.FirstOrDefault(d => d.VendorID.ToLower() == vendorId && d.ProductID.ToLower() == deviceId
                                    && ((index is null && version is null) || (d.DeviceIndex == index && d.Version == version))), profile.Path);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Invalid arguments: " + ex.Message);
                    }
                }
            }
        }
    }
}

