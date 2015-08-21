using Neo4jClient;
using System;
using System.Linq;
using System.Windows;
using WmiLight;

namespace WPFAssets
{
    public class Owner
    {
        public string MSAlias { get; set; }
        public string WSAlias { get; set; }
        public string Email { get; set; }
    }

    public class Device
    {
        public string WSAsset { get; set; }
        public string AssetTag { get; set; }
        public string Manufacturer { get; set; }
        public string SN { get; set; }
        public string CPU { get; set; }
        public string RAM { get; set; }
        public string Disk { get; set; }
        public string SystemDate { get; set; }
        public string OS { get; set; }
        public string OSInstallDate { get; set; }
        public string Model { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PopulateInfo();
        }

        private void PopulateInfo()
        {
            try
            {
                // Populate owner information.
                User.Text = Environment.GetEnvironmentVariable("USERNAME");

                // Enable remote access so owner can add all computers on one box.
                using (WmiConnection conncetion = new WmiConnection(@"\\" + RemoteDevice.Text + @"\root\cimv2"))
                {
                    string temp = string.Empty;
                    foreach (WmiObject process in conncetion.CreateQuery("SELECT * FROM Win32_BIOS"))
                    {
                        temp += process["Manufacturer"];
                    }
                    Manufacturer.Text = temp.Trim();

                    temp = string.Empty;
                    foreach (WmiObject process in conncetion.CreateQuery("SELECT * FROM Win32_BIOS"))
                    {
                        temp += process["SerialNumber"];
                    }
                    SN.Text = temp.Trim();

                    temp = string.Empty;
                    foreach (WmiObject process in conncetion.CreateQuery("SELECT * FROM Win32_Processor"))
                    {
                        temp += process["Name"] + " - " + process["NumberOfCores"] + " core(s)" + " " + process["NumberOfLogicalProcessors"] + " thread(s)";
                    }
                    CPU.Text = temp.Trim();

                    long value = 0;
                    foreach (WmiObject process in conncetion.CreateQuery("SELECT * FROM Win32_PhysicalMemory"))
                    {
                        value += Convert.ToInt64(process["Capacity"]);
                    }
                    RAM.Text = value.ToString();

                    value = 0;
                    foreach (WmiObject process in conncetion.CreateQuery("SELECT * FROM Win32_LogicalDisk"))
                    {
                        value += Convert.ToInt64(process["Size"]);
                    }
                    Disk.Text = value.ToString();

                    temp = string.Empty;
                    foreach (WmiObject process in conncetion.CreateQuery("SELECT * FROM Win32_BIOS"))
                    {
                        temp += process["ReleaseDate"];
                    }
                    SystemDate.Text = temp.Trim();

                    temp = string.Empty;
                    foreach (WmiObject process in conncetion.CreateQuery("SELECT * FROM Win32_OperatingSystem"))
                    {
                        temp += process["Caption"] + " - Service Pack " + process["ServicePackMajorVersion"] + "." + process["ServicePackMinorVersion"];
                    }
                    OS.Text = temp.Trim();

                    temp = string.Empty;
                    foreach (WmiObject process in conncetion.CreateQuery("SELECT * FROM Win32_OperatingSystem"))
                    {
                        temp += process["InstallDate"];
                    }
                    OSInstallDate.Text = temp.Trim();

                    temp = string.Empty;
                    foreach (WmiObject process in conncetion.CreateQuery("SELECT * FROM Win32_ComputerSystem"))
                    {
                        temp += process["Model"];
                    }
                    Model.Text = temp.Trim();
                }
            }
            catch (Exception e)
            {
                Message.Text = e.Message;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Save.IsEnabled = false;
            try
            {
                if (Model.Text.Contains("Virtual"))
                {
                    throw new Exception("Virtual machine is not a valid device model.");
                }

                long wsAsset = 0;

                foreach (var asset in Asset.Text.Split(','))
                {
                    if (!string.Equals(asset, "0") &&
                        (asset.Length != 8 || !long.TryParse(asset, out wsAsset)))
                    {
                        throw new Exception("Invalid old WS Asset tag, expect 8-digit number or 0.");
                    }
                }

                foreach (var asset in AssetTag.Text.Split(','))
                {
                    if (!string.IsNullOrWhiteSpace(asset) &&
                    (asset.Length != 12 || !long.TryParse(asset, out wsAsset)))
                    {
                        throw new Exception("Invalid new WS Asset tag, expect 12-digit number or null.");
                    }
                }

                string msAlias = User.Text.Trim().ToLower();
                string wsAlias = string.Empty;
                wsAlias = msAlias;

                string sn = SN.Text.Trim();
                if (string.IsNullOrWhiteSpace(sn) || string.Equals(sn.ToLower(), "serial number"))
                {
                    throw new Exception("SN is invalid. Use WS Asset tag if you have no idea.");
                }

                var client = new GraphClient(new Uri("http://neo4j:111111@localhost:7474/db/data"));
                client.Connect();

                Device newDevice = new Device
                {
                    WSAsset = Asset.Text.Trim(),
                    AssetTag = AssetTag.Text.Trim(),
                    Manufacturer = Manufacturer.Text.Trim(),
                    SN = SN.Text.Trim().ToUpper(),
                    CPU = CPU.Text.Trim(),
                    RAM = RAM.Text.Trim(),
                    Disk = Disk.Text.Trim(),
                    SystemDate = SystemDate.Text.Trim(),
                    OS = OS.Text.Trim(),
                    OSInstallDate = OSInstallDate.Text.Trim(),
                    Model = Model.Text.Trim()
                };

                // Don't create duplicate relationship!
                var result = client.Cypher
                    .Match("(device:Device)")
                    .Where("device.SN = {sn}")
                    .WithParam("sn", newDevice.SN)
                    .Return(device => device.As<Node<Device>>());

                if (result.Results.Count() != 0)
                {
                    throw new Exception("SN must be unique. Use WS Asset tag if you have no idea.");
                }

                if (!string.Equals(newDevice.WSAsset, "0"))
                {
                    result = client.Cypher
                        .Match("(device:Device)")
                        .Where("device.WSAsset = {asset}")
                        .WithParam("asset", newDevice.WSAsset)
                        .Return(device => device.As<Node<Device>>());

                    if (result.Results.Count() != 0)
                    {
                        throw new Exception("Old WS asset must be unique.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(newDevice.AssetTag))
                {
                    result = client.Cypher
                        .Match("(device:Device)")
                        .Where("device.AssetTag = {asset}")
                        .WithParam("asset", newDevice.AssetTag)
                        .Return(device => device.As<Node<Device>>());

                    if (result.Results.Count() != 0)
                    {
                        throw new Exception("New WS asset must be unique.");
                    }
                }

                // Create device if not exist
                client.Cypher
                    .Merge("(device:Device { SN: {sn} })")
                    .OnCreate()
                    .Set("device = {newDevice}")
                    .WithParams(new
                    {
                        sn = newDevice.SN,
                        newDevice
                    })
                    .ExecuteWithoutResults();

                Owner newOwner = new WPFAssets.Owner
                {
                    MSAlias = msAlias,
                    WSAlias = wsAlias,
                    Email = msAlias + "@ms;" + wsAlias + "@ws"
                };

                // Create owner if not exist.
                client.Cypher
                    .Merge("(owner:Owner { MSAlias: {msalias} })")
                    .OnCreate()
                    .Set("owner = {newOwner}")
                    .WithParams(new
                    {
                        msalias = newOwner.MSAlias,
                        newOwner
                    })
                    .ExecuteWithoutResults();

                // Relate owner and device.
                client.Cypher
                    .Match("(device:Device)", "(owner:Owner)")
                    .Where((Device device) => device.SN == newDevice.SN)
                    .AndWhere((Owner owner) => owner.MSAlias == newOwner.MSAlias)
                    .CreateUnique("device<-[:OWNS]-owner")
                    .ExecuteWithoutResults();

                Message.Text = newDevice.SN + " is owned by " + newOwner.MSAlias;
                SN.Text = newDevice.SN;
                User.Text = newOwner.MSAlias;
            }
            catch (Exception exception)
            {
                Message.Text = exception.Message;
            }
            Save.IsEnabled = true;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            Connect.IsEnabled = false;
            Message.Text = "Connecting to " + RemoteDevice.Text;
            PopulateInfo();
            Connect.IsEnabled = true;
            Message.Text = "Connected to " + RemoteDevice.Text;
        }

        private void OtherDevices_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Enable remote access so owner can add all computers on one box.
                using (WmiConnection conncetion = new WmiConnection(@"\\" + RemoteDevice.Text + @"\root\WMI"))
                {
                    string temp = string.Empty;
                    foreach (WmiObject process in conncetion.CreateQuery("SELECT * FROM WmiMonitorID"))
                    {
                        int[] tempArray = (int[])process["ManufacturerName"];
                        foreach (int c in tempArray)
                        {
                            if ((char)c == '\0')
                            {
                                continue;
                            }
                            temp += ((char)c).ToString();
                        }
                        temp += ",";
                    }
                    Manufacturer.Text = temp.Substring(0, temp.Length - 1);

                    temp = string.Empty;
                    foreach (WmiObject process in conncetion.CreateQuery("SELECT * FROM WmiMonitorID"))
                    {
                        int[] tempArray = (int[])process["SerialNumberID"];
                        foreach (int c in tempArray)
                        {
                            if ((char)c == '\0')
                            {
                                continue;
                            }
                            temp += ((char)c).ToString();
                        }
                        temp += ",";
                    }
                    SN.Text = temp.Substring(0, temp.Length - 1);

                    temp = string.Empty;
                    foreach (WmiObject process in conncetion.CreateQuery("SELECT * FROM WmiMonitorID"))
                    {
                        int[] tempArray = (int[])process["UserFriendlyName"];
                        foreach (int c in tempArray)
                        {
                            if ((char)c == '\0')
                            {
                                continue;
                            }
                            temp += ((char)c).ToString();
                        }
                        temp += ",";
                    }
                    Model.Text = temp.Substring(0, temp.Length - 1);

                    CPU.Text = string.Empty;
                    RAM.Text = string.Empty;
                    Disk.Text = string.Empty;
                    SystemDate.Text = string.Empty;
                    OS.Text = string.Empty;
                    OSInstallDate.Text = string.Empty;
                }
            }
            catch (Exception exception)
            {
                Message.Text = exception.Message;
            }
        }
    }
}
