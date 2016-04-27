using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace HTTPDataAnalyzer.Registration
{
    public class RecentApp
    {
        [JsonProperty("collecteddate")]
        public string CollectedDate { get; set; }
        [JsonProperty("isinstalled")]
        public bool IsInstalled { get; set; }
        [JsonProperty("installedapps")]
        public winaudits.InstalledApp AppDetails { get; set; }
    }

    public class RecentAutoRuns
    {
        [JsonProperty("collecteddate")]
        public DateTime CollectedDate { get; set; }
        [JsonProperty("isdeleted")]
        public bool IsDeleted { get; set; }
        [JsonProperty("installedapps")]
        public winaudits.Autorunpoints AppDetails { get; set; }
    }

    public class NetworkType
    {
        [JsonProperty("ipadress")]
        public string IPAdress { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("mac")]
        public string MAC { get; set; }
    }

    public class SystemConfiguration
    {
        [JsonProperty("hostname")]
        public string HostName { get; set; }
        [JsonProperty("agentid")]
        public int AgentID { get; set; }
        [JsonProperty("agentversion")]
        public string AgentVersion { get; set; }
        [JsonProperty("timezone")]
        public string TimeZone { get; set; }
        [JsonProperty("bitlevel")]
        public int BitLevel { get; set; }
        [JsonProperty("osedition")]
        public string OSEdition { get; set; }
        [JsonProperty("osservicepack")]
        public string OSServicePack { get; set; }
        [JsonProperty("osname")]
        public string OSName { get; set; }
        [JsonProperty("oslastuptime")]
        public DateTime OSLastUpTime { get; set; }
        [JsonProperty("domainname")]
        public string DomainName { get; set; }
        [JsonProperty("installdate")]
        public DateTime Installdate { get; set; }
        [JsonProperty("productid")]
        public string ProductID { get; set; }
        [JsonProperty("processor")]
        public string Processor { get; set; }
        [JsonProperty("primaryuser")]
        public string Primaryuser { get; set; }
        [JsonProperty("registereduser")]
        public string Registereduser { get; set; }
        [JsonProperty("acrobat")]
        public string Acrobat { get; set; }
        [JsonProperty("java")]
        public string Java { get; set; }
        [JsonProperty("flash")]
        public string Flash { get; set; }
        [JsonProperty("chasistype")]
        public string ChasisType { get; set; }

        public List<NetworkType> NetworkTypes { get; set; }
        public List<Browser> Browsers { get; set; }
        public List<OfficeApplication> OfficeApplications { get; set; }
        public List<RecentAutoRuns> Autorunpoints { get; set; }
        public List<RecentApp> RecentApps { get; set; }
    }

    public class ConfigFinder
    {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(uint DestIP, uint SrcIP, byte[] pMacAddr, ref int PhyAddrLen);

        static Dictionary<string, RecentApp> PreviousInstalledApp = new Dictionary<string, RecentApp>();
        //static Dictionary<string, RecentAutoRuns> PreviousAutorunpoints = new Dictionary<string, RecentAutoRuns>();

        public static byte[] GetConfig(bool isRegister)
        {
            string temp = string.Empty;
            Dictionary<string, RecentApp> currentAppList = new Dictionary<string, RecentApp>();
            SystemConfiguration sysConfig = null;
            if (isRegister)
            {
                sysConfig = HTTPDataAnalyzer.ReadQueries.GetSystemConfiguration();
            }

           // return serial;
            if (sysConfig == null)
            {
                sysConfig = new SystemConfiguration();
                sysConfig.NetworkTypes = new List<NetworkType>();
                sysConfig.RecentApps = new List<RecentApp>();
                sysConfig.Autorunpoints = new List<RecentAutoRuns>();
                sysConfig.Browsers = BrowsersDetector.GetBrowsers().Select(x => x.Value).ToList();
                sysConfig.OfficeApplications = MSOfficeDetector.GetOfficeApps();

                Dictionary<string, RecentAutoRuns> currentAutoruns = new Dictionary<string, RecentAutoRuns>();
                foreach (var item in winaudits.AutoRunManager.StartAudit())
                {
                    RecentAutoRuns recent = new RecentAutoRuns();
                    recent.AppDetails = item;
                    recent.IsDeleted = false;
                    recent.CollectedDate = DateTime.UtcNow;
                    sysConfig.Autorunpoints.Add(recent);
                }

                foreach (var item in winaudits.InstalledAppAuditor.StartAudit())
                {
                    RecentApp recent = new RecentApp();
                    recent.AppDetails = item;
                    recent.IsInstalled = true;
                    recent.CollectedDate = DateTime.UtcNow.ToString(HTTPDataAnalyzer.DBManager.DateTimeFormat);
                    sysConfig.RecentApps.Add(recent);
                    try
                    {
                        if (!currentAppList.ContainsKey(recent.AppDetails.Key))
                        {
                            currentAppList.Add(recent.AppDetails.Key, recent);
                        }
                        else
                        {
                            //Registration.ClientRegistrar.Logger.Error("=====================================================================");
                            //Registration.ClientRegistrar.Logger.Error("Config 1");
                            //Registration.ClientRegistrar.Logger.Error(recent.AppDetails.DisplayName);
                            //Registration.ClientRegistrar.Logger.Error(recent.AppDetails.Key);
                            //Registration.ClientRegistrar.Logger.Error(recent.AppDetails.InstallDate);
                            //Registration.ClientRegistrar.Logger.Error(recent.AppDetails.Version);
                            //Registration.ClientRegistrar.Logger.Error(recent.AppDetails.Is64);
                            //Registration.ClientRegistrar.Logger.Error(recent.IsInstalled);
                            //Registration.ClientRegistrar.Logger.Error(recent.CollectedDate);

                            recent = currentAppList[recent.AppDetails.Key];
                            //Registration.ClientRegistrar.Logger.Error("Config 2");
                            //Registration.ClientRegistrar.Logger.Error(recent.AppDetails.DisplayName);
                            //Registration.ClientRegistrar.Logger.Error(recent.AppDetails.Key);
                            //Registration.ClientRegistrar.Logger.Error(recent.AppDetails.InstallDate);
                            //Registration.ClientRegistrar.Logger.Error(recent.AppDetails.Version);
                            //Registration.ClientRegistrar.Logger.Error(recent.AppDetails.Is64);
                            //Registration.ClientRegistrar.Logger.Error(recent.IsInstalled);
                            //Registration.ClientRegistrar.Logger.Error(recent.CollectedDate);
                            //Registration.ClientRegistrar.Logger.Error("=====================================================================");
                        }
                    }
                    catch (Exception ex)
                    {
                        //Registration.ClientRegistrar.Logger.Error(ex);
                    }
                }

                sysConfig.ChasisType = ChasisTypeFinder.GetChassisType().ToString();
                sysConfig.AgentVersion = "0";
                sysConfig.TimeZone = GetSystemTimeZoneName();
                sysConfig.HostName = Dns.GetHostName();
                sysConfig.DomainName = Environment.UserDomainName;

                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (nic.OperationalStatus.ToString().ToLower() == "up" && nic.NetworkInterfaceType.ToString().ToLower() != "loopback")
                    {
                        NetworkType netTpe = new NetworkType();

                        foreach (UnicastIPAddressInformation ip in nic.GetIPProperties().UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                netTpe.IPAdress = ip.Address.ToString();
                                break;
                            }
                        }
                        if (netTpe.IPAdress == null)
                        {
                            continue;
                        }
                        netTpe.MAC = nic.GetPhysicalAddress().ToString();
                        netTpe.Type = nic.NetworkInterfaceType.ToString();
                        sysConfig.NetworkTypes.Add(netTpe);
                    }
                }

                sysConfig.Processor = GetProcessor();
                sysConfig.BitLevel = OSInfo.Bits;
                sysConfig.OSEdition = OSInfo.Edition;
                sysConfig.OSServicePack = OSInfo.ServicePack;
                sysConfig.OSName = OSInfo.Name;
                sysConfig.ProductID =   ProductID();
                sysConfig.AgentID= ConfigHandler.Config.AgentInstaller.AgentID;
                sysConfig.AgentVersion = ConfigHandler.Config.AgentInstaller.AgentVersion;
                sysConfig.OSLastUpTime = LastBootUpTime();
                sysConfig.Installdate = DateTime.UtcNow;
                sysConfig.Acrobat = GetAdobeVer();
                sysConfig.Java = GetJavaVersion();
                sysConfig.Registereduser = Util.GetWindowsUserName();
                sysConfig.Flash = GetFlashPlayerVersionString().Replace(",", ".");
            }
            if (isRegister)
            {
                HTTPDataAnalyzer.InsertQueries.InsertInHostInfo(sysConfig);
                sysConfig.RecentApps.Clear();
                sysConfig.Autorunpoints.Clear();
            }
            else
            {
                if (PreviousInstalledApp.Count == 0)
                {
                    PreviousInstalledApp = HTTPDataAnalyzer.ReadQueries.GetPreviousConfig();
                }

                if (PreviousInstalledApp.Count > 0)
                {
                    List<RecentApp> tempInstalledApp = new List<RecentApp>();
                    tempInstalledApp.AddRange(sysConfig.RecentApps);
                    sysConfig.RecentApps.Clear();
                    for (int i = 0; i < tempInstalledApp.Count; i++)
                    {
                        var appName = tempInstalledApp[i];
                        if (PreviousInstalledApp.ContainsKey(appName.AppDetails.Key))
                        {
                            continue;
                        }

                        sysConfig.RecentApps.Add(appName);
                        PreviousInstalledApp.Add(appName.AppDetails.Key, appName);
                    }

                    foreach (var item in PreviousInstalledApp)
                    {
                        if (!currentAppList.ContainsKey(item.Key))
                        {
                            RecentApp unInstall = PreviousInstalledApp[item.Key];
                            unInstall.IsInstalled = false;
                            unInstall.CollectedDate = DateTime.UtcNow.ToString(HTTPDataAnalyzer.DBManager.DateTimeFormat);
                            sysConfig.RecentApps.Add(unInstall);
                        }
                    }

                    foreach (RecentApp item in sysConfig.RecentApps)
                    {
                        if (!item.IsInstalled)
                        {
                            if (PreviousInstalledApp.ContainsKey(item.AppDetails.Key))
                            {
                                PreviousInstalledApp.Remove(item.AppDetails.Key);
                            }
                        }
                    }
                }

                //if (PreviousAutorunpoints.Count == 0)
                //{
                //    PreviousAutorunpoints = HTTPDataAnalyzer.DBManager.ReadQueries.GetPreviousAutoRuns();
                //}

                //if (PreviousAutorunpoints.Count > 0)
                //{
                //    List<RecentAutoRuns> tempAutoRuns = new List<RecentAutoRuns>();
                //    tempAutoRuns.AddRange(sysConfig.Autorunpoints);
                //    sysConfig.Autorunpoints.Clear();
                //    for (int i = 0; i < tempAutoRuns.Count; i++)
                //    {
                //        var appName = tempAutoRuns[i];
                //        if (PreviousAutorunpoints.ContainsKey(appName.AppDetails.FilePath))
                //        {
                //            continue;
                //        }

                //        sysConfig.Autorunpoints.Add(appName);
                //        PreviousAutorunpoints.Add(appName.AppDetails.FilePath, appName);
                //    }

                //    foreach (var item in PreviousAutorunpoints)
                //    {
                //        if (!currentAutoruns.ContainsKey(item.Key))
                //        {
                //            RecentAutoRuns unInstall = PreviousAutorunpoints[item.Key];
                //            unInstall.IsDeleted = false;
                //            sysConfig.Autorunpoints.Add(unInstall);
                //        }
                //    }
                //}

                HTTPDataAnalyzer.UpdateQuery.UpdateInHostInfo(sysConfig);
            }          

            byte[] configByte = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(sysConfig));

            return configByte;
        }

        private static string ProductID()
        {
            string productId = string.Empty;
            try
            {
                string queryString = "SELECT SerialNumber FROM Win32_OperatingSystem";

                productId = (from ManagementObject managementObject in new ManagementObjectSearcher(queryString).Get()
                             from PropertyData propertyData in managementObject.Properties
                             where propertyData.Name == "SerialNumber"
                             select (string)propertyData.Value).FirstOrDefault();
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
            }

            return productId;
        }

        private static string GetSystemTimeZoneName()
        {
            System.Globalization.CultureInfo oCulture = System.Globalization.CultureInfo.InstalledUICulture;// System.Threading.Thread.CurrentThread.CurrentCulture; 
            if (oCulture.TwoLetterISOLanguageName != "en")
            {
                TimeZone zone1 = TimeZone.CurrentTimeZone;
                ICollection<TimeZoneInfo> zones = TimeZoneInfo.GetSystemTimeZones();

                foreach (TimeZoneInfo zoneInfo in zones)
                {
                    if (zoneInfo.StandardName == zone1.StandardName && zoneInfo.DaylightName == zone1.DaylightName)
                    {
                        return zoneInfo.Id;
                    }
                }
            }

            return TimeZone.CurrentTimeZone.StandardName;
        }

        private static string GetJavaVersion()
        {
            string currentVerion = "";
            try
            {
                using (RegistryKey basekey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32))
                {
                    using (RegistryKey subKey = basekey.OpenSubKey("SOFTWARE\\JavaSoft\\Java Runtime Environment"))
                    {
                        if (subKey != null)
                        {
                            currentVerion = subKey.GetValue("CurrentVersion").ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
            }

            return currentVerion;
        }

        private static string GetAdobeVer()
        {
            string acroVers = string.Empty;
            try
            {
                RegistryKey software = Registry.LocalMachine.OpenSubKey("Software");

                if (software != null)
                {
                    RegistryKey adobe = null;

                    if (Environment.Is64BitOperatingSystem)
                    {
                        software = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);

                        if (software != null)
                            adobe = software.OpenSubKey(@"SOFTWARE\\Wow6432Node\\Adobe");
                    }
                    else
                    {
                        software = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);

                        if (software != null)
                            adobe = software.OpenSubKey(@"SOFTWARE\\Adobe");
                    }

                    if (adobe != null)
                    {
                        RegistryKey acroRead = adobe.OpenSubKey("Acrobat Reader");

                        if (acroRead != null)
                        {
                            string[] acroReadVersions = acroRead.GetSubKeyNames();

                            foreach (string versionNumber in acroReadVersions)
                            {
                                acroVers = versionNumber;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
            }
            return acroVers;
        }

        private static string GetFlashPlayerVersionString()
        {
            try
            {
                using (RegistryKey basekey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32))
                {
                    using (RegistryKey regKey = basekey.OpenSubKey(@"SOFTWARE\Macromedia\FlashPlayer"))
                    {
                        if (regKey != null)
                        {
                            return Convert.ToString(regKey.GetValue("CurrentVersion"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
            }
            return string.Empty;
        }

        private static DateTime LastBootUpTime()
        {
            DateTime dtBootTime = new DateTime();
            SelectQuery query = new SelectQuery(@"SELECT LastBootUpTime FROM Win32_OperatingSystem WHERE Primary='true'");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);

            foreach (ManagementObject mo in searcher.Get())
            {
                dtBootTime = ManagementDateTimeConverter.ToDateTime(mo.Properties["LastBootUpTime"].Value.ToString());
                return dtBootTime.ToUniversalTime();
            }
            return dtBootTime.ToUniversalTime();
        }

        private static string GetMAC(string ip)
        {
            IPAddress dst = IPAddress.Parse(ip); // the destination IP address Note:Can Someone give the code to get the IP address of the server

            uint uintAddress = BitConverter.ToUInt32(dst.GetAddressBytes(), 0);
            byte[] macAddr = new byte[6];
            int macAddrLen = macAddr.Length;
            if (SendARP(uintAddress, 0, macAddr, ref macAddrLen) != 0)
            {
                //Registration.ClientRegistrar.Logger.Error("SendARP failed.");
                return string.Empty;
            }

            string[] str = new string[(int)macAddrLen];
            for (int i = 0; i < macAddrLen; i++)
            {
                str[i] = macAddr[i].ToString("x2");
            }
            return string.Join(":", str);
        }

        private static string GetProcessor()
        {
            string procName = string.Empty;
            try
            {
                using (ManagementObjectSearcher win32Proc = new ManagementObjectSearcher("select * from Win32_Processor"))
                {
                    foreach (ManagementObject obj in win32Proc.Get())
                    {
                        procName = obj["Name"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
            }

            return procName;
        }
    }
}
