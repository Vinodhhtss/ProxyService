using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace winaudits
{
    public class InstalledApp
    {
        [JsonProperty("displayname")]
        public string DisplayName { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("installdate")]
        public string InstallDate { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("is64")]
        public bool Is64 { get; set; }
    }

    public class InstalledAppAuditor
    {
        public static List<InstalledApp> StartAudit()
        {
            var reginstalllist = new List<InstalledApp>();
            try
            {

                string[] subkeys_wow = RegistryUtil.GetSubKeys("LocalMachine", "Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall", false);
                    if (subkeys_wow != null)
                    {
                        foreach (var sk in subkeys_wow)
                        {

                            InstalledApp rg = new InstalledApp();
                        string subk2_wow = "Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + sk.ToString();
                            rg.DisplayName = RegistryUtil.GetStringSubValue("LocalMachine", subk2_wow, "DisplayName", false);
                            if (string.IsNullOrEmpty(rg.DisplayName))
                            {
                                continue;
                            }
                            rg.Version = RegistryUtil.GetStringSubValue("LocalMachine", subk2_wow, "DisplayVersion", false);
                            rg.InstallDate = RegistryUtil.GetStringSubValue("LocalMachine", subk2_wow, "InstallDate", false);
                            rg.InstallDate = GetValidDate(rg.InstallDate);

                            rg.Key = "LocalMachine\\" + subk2_wow;
                            rg.Is64 = false;
                            reginstalllist.Add(rg);
                        }
                    }        

                ////Proceed only for 64 bit
                if (PlatformCheck.IsWow64())
                {

                    string[] subkeys_64 = RegistryUtil.GetSubKeys("LocalMachine", "Software\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall", true);
                    if (subkeys_64 != null)
                    {
                        foreach (var sk in subkeys_64)
                        {
                            InstalledApp rg = new InstalledApp();
                            string subk2_64 = "Software\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + sk.ToString();
                            rg.DisplayName = RegistryUtil.GetStringSubValue("LocalMachine", subk2_64, "DisplayName", true);
                            if (string.IsNullOrEmpty(rg.DisplayName))
                            {
                                continue;
                            }
                            rg.Version = RegistryUtil.GetStringSubValue("LocalMachine", subk2_64, "DisplayVersion", true);
                            rg.InstallDate = RegistryUtil.GetStringSubValue("LocalMachine", subk2_64, "InstallDate", true);
                            rg.InstallDate = GetValidDate(rg.InstallDate);
                            rg.Key = "LocalMachine\\" + subk2_64;
                            rg.Is64 = true;
                            reginstalllist.Add(rg);
                        }
                    }
                }
                List<string> regprof = RegistryUtil.GetRegProfiles();

                foreach (var prf in regprof)
                {

                    string[] users = RegistryUtil.GetSubKeys(prf, "Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall", true);
                    if (users != null)
                    {
                        foreach (var sk in users)
                        {
                            InstalledApp rg = new InstalledApp();
                            string innnerKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\" + sk.ToString();
                            rg.DisplayName = RegistryUtil.GetStringSubValue(prf, innnerKey, "DisplayName", true);
                            if (string.IsNullOrEmpty(rg.DisplayName))
                            {
                                continue;
                            }
                            rg.Version = RegistryUtil.GetStringSubValue(prf, innnerKey, "DisplayVersion", true);
                            rg.InstallDate = RegistryUtil.GetStringSubValue(prf, innnerKey, "InstallDate", true);
                            rg.InstallDate = GetValidDate(rg.InstallDate);
                            rg.Key = prf + "\\" + innnerKey;
                            reginstalllist.Add(rg);
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

            return reginstalllist;
        }

        private static string GetValidDate(string date)
        {
            if (!string.IsNullOrEmpty(date))
            {
                DateTime dt;
                try
                {

                    if (date.IndexOf("\\") > 0)
                    {
                        DateTime.TryParse(date, out dt);
                        return dt.ToUniversalTime().ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        if (date.ToCharArray().Length == 8)
                        {
                            dt = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture);
                            return dt.ToUniversalTime().ToString("dd/MM/yyyy");
                        }
                    }
                }
                catch (Exception)
                {

                }
            }
            return date;
        }
    }
}
