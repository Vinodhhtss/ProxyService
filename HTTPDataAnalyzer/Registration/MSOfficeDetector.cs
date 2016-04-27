using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace HTTPDataAnalyzer.Registration
{
    public enum OfficeComponent
    {
        Word,
        Excel,
        PowerPoint,
        Outlook
    }
    
    public class OfficeApplication
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
    }

    class MSOfficeDetector
    {
        public static List<OfficeApplication> GetOfficeApps()
        {
            List<OfficeApplication> listOfficeApps = new List<OfficeApplication>();
            
            try
            {
                foreach (var item in Enum.GetValues(typeof(OfficeComponent)))
                {
                    OfficeApplication officeApp = new OfficeApplication();
                    switch ((OfficeComponent)item)
                    {
                        case OfficeComponent.Word:
                            officeApp.Name = "winword.exe";
                            break;
                        case OfficeComponent.Excel:
                            officeApp.Name = "excel.exe";
                            break;
                        case OfficeComponent.PowerPoint:
                            officeApp.Name = "powerpnt.exe";
                            break;
                        case OfficeComponent.Outlook:
                            officeApp.Name = "outlook.exe";
                            break;
                    }
                    
                    RegistryKey officeKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + officeApp.Name);
                    if (officeKey != null)
                    {
                        string path = officeKey.GetValue(null).ToString();
                        string majorVersion = GetProductMajorVersion(path);
                        switch (majorVersion)
                        {
                            case "11.0":
                            case "11":
                                officeApp.Version = "MS Office 2003";
                                break;
                            case "12.0":
                            case "12":
                                officeApp.Version = "MS Office 2007";
                                break;
                            case "14.0":
                            case "14":
                                officeApp.Version = "MS Office 2010";
                                break;
                            case "15.0":
                            case "15":
                                officeApp.Version = "MS Office 2013";
                                break;
                            case "16.0":
                            case "16":
                                officeApp.Version = "MS Office 2016";
                                break;
                            default:
                                officeApp.Version = GetProductName(path);
                                break;
                        }
                        officeKey.Close();
                    }
                    if (!string.IsNullOrEmpty(officeApp.Version))
                    {
                        listOfficeApps.Add(officeApp);
                    }
                }
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
            }
            return listOfficeApps;
        }

        private static string GetProductName(string _path)
        {
            string productName = string.Empty;
            if (File.Exists(_path))
            {
                try
                {
                    FileVersionInfo _fileVersion = FileVersionInfo.GetVersionInfo(_path);
                    productName = _fileVersion.ProductName;
                }
                catch (Exception ex)
                {
                    //Registration.ClientRegistrar.Logger.Error(ex);
                }
            }
            return productName;
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
                catch (Exception ex)
                {
                    //Registration.ClientRegistrar.Logger.Error(ex);
                }
            }
            return productName;
        }
    }
}
