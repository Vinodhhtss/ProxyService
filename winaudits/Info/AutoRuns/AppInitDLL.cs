using Microsoft.Win32;
using System;
using System.Collections.Generic;

namespace winaudits
{
    internal class AppInitDLL
    {
        public static List<Autorunpoints> StartAudit()
        {
            var lstAutoRuns = new List<Autorunpoints>();
            char[] delim = { ',' };
            ReadHiveValue("LocalMachine", "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Windows", "AppInit_Dlls", delim, false, true, "APPINIT", lstAutoRuns);
            ReadHiveValue("LocalMachine", "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Windows", "AppInit_Dlls", delim, true, true, "APPINIT64", lstAutoRuns);

            return lstAutoRuns;
        }

        private static void ReadHiveValue(string hive, string key, string valname, char[] delim, bool is64, bool type, string runtype, List<Autorunpoints> lstAutoRuns)
        {
            try
            {
                RegistryKey basekey = null;
                RegistryKey runkey = null;

                if (hive == "LocalMachine")
                {
                    basekey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, is64 == true ? RegistryView.Registry64 : RegistryView.Registry32);
                    runkey = basekey.OpenSubKey(key);
                }
                else
                {
                    RegistryKey runhive = Registry.Users.OpenSubKey(hive);
                    if (runhive != null)
                    {
                        runkey = runhive.OpenSubKey(key);
                    }
                }
                string owner = RegistryUtil.GetRegKeyOwner(runkey);
                if (runkey != null)
                {
                    try
                    {
                        string keyValue = Convert.ToString(runkey.GetValue(valname));
                        if (!string.IsNullOrEmpty(keyValue))
                        {
                            string[] vals = keyValue.Split(delim);

                            foreach (var valstring in vals)
                            {
                                Autorunpoints runPoint = new Autorunpoints();
                                runPoint.Type = "AppInit_Dlls";
                                if (hive == "LocalMachine")
                                {
                                    runPoint.RegistryPath = "LocalMachine\\" + key;
                                }
                                else
                                {
                                    runPoint.RegistryPath = hive + "\\" + key;
                                }

                                runPoint.RegistryValueName = valname;
                                runPoint.RegistryValueString = valstring;
                                runPoint.FilePath = valstring;
                                runPoint.RegistryOwner = owner;
                                lstAutoRuns.Add(runPoint);
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                if (basekey != null)
                {
                    basekey.Close();
                }

                if (runkey != null)
                {
                    runkey.Close();
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
