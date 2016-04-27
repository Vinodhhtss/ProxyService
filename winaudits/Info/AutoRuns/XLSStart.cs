using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace winaudits
{
   internal  class XLSStart
    {
        public static List<Autorunpoints> StartAudit()
        {
            List<Autorunpoints> xlselements = new List<Autorunpoints>();
            try
            {
                string sysdrv = Environment.GetEnvironmentVariable("SystemDrive");

                List<string> ls = RegistryUtil.GetUserProfiles();

                if (ls != null && ls.Count > 0)
                {
                    for (int i = 0; i < ls.Count; i++)
                    {
                        try
                        {
                            RegistryKey officeKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\excel.exe");
                            if (officeKey != null)
                            {
                                string path = officeKey.GetValue(null).ToString();
                                string majorVersion = GetProductMajorVersion(path);
                                if (!majorVersion.EndsWith(".0"))
                                {
                                    majorVersion += ".0";
                                }
                                string tempRegis = ls[i] + "\\Software\\Microsoft\\Office\\" + majorVersion + "\\Excel\\Security\\Trusted Locations\\";
                                RegistryKey trustedLocKey = Registry.Users.OpenSubKey(tempRegis);
                              
                                if (trustedLocKey == null)
                                {
                                    continue;
                                }
                                DateTime regMod = RegistryModified.lastWriteTime(trustedLocKey);
                                string[] trustedLocations = trustedLocKey.GetSubKeyNames();
                                foreach (var item in trustedLocations)
                                {
                                    string slocation = tempRegis + item;
                                    AddFiles(xlselements, slocation, regMod);
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
            catch (Exception)
            {

            }

            return xlselements;
        }

        private static void AddFiles(List<Autorunpoints> xlselements, string location, DateTime registryMod)
        {
            string xlsfolder;
            string owner;
            using (RegistryKey reglocation = Registry.Users.OpenSubKey(location))
            {
                xlsfolder = reglocation.GetValue("Path").ToString();
                owner = RegistryUtil.GetRegKeyOwner(reglocation);
            }

            if (Directory.Exists(xlsfolder) == true)
            {
                foreach (var file in Directory.GetFiles(xlsfolder))
                {
                    Autorunpoints autopoint = new Autorunpoints();
                    autopoint.Type = "XLSStart";
                    autopoint.IsFile = true;
                    autopoint.FilePath = file;
                    autopoint.RegistryPath = location;
                    autopoint.RegistryValueName = "Path";
                    autopoint.RegistryValueString = xlsfolder;
                    autopoint.RegistryOwner = owner;
                    autopoint.RegistryModified = registryMod.ToString(DBManager.DateTimeFormat);
                    xlselements.Add(autopoint);
                }
            }
        }

        private static string GetProductMajorVersion(string _path)
        {
            string productName = string.Empty;
            if (File.Exists(_path))
            {
                try
                {
                    FileVersionInfo _fileVersion = FileVersionInfo.GetVersionInfo(_path);
                    productName = _fileVersion.ProductMajorPart.ToString();
                }
                catch (Exception)
                {
                }
            }
            return productName;
        }
    }
}