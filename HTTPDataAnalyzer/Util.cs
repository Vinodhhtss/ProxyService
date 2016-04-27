using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;

namespace HTTPDataAnalyzer
{
    class Util
    {
        public static string FindIpAdrress()
        {
            string myIP = string.Empty;
            try
            {
                string hostName = Dns.GetHostName();
                IPAddress[] ipAddresses = Dns.GetHostEntry(hostName).AddressList;

                foreach (var ipAddress in ipAddresses)
                {
                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ipAddress))
                    {
                        myIP = ipAddress.ToString();
                        break;
                    }
                }
                if (myIP == string.Empty)
                {
                    myIP = IPAddress.Loopback.ToString();
                }
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
            }

            return myIP;
        }

        public static string GetValidIPAddress(string hostName)
        {
            string myIP = string.Empty;
            try
            {
                IPAddress[] ipAddresses = Dns.GetHostEntry(hostName).AddressList;
                foreach (var ipAddress in ipAddresses)
                {
                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ipAddress))
                    {
                        myIP = ipAddress.ToString();
                        break;
                    }
                }
                if (myIP == string.Empty)
                {
                    myIP = IPAddress.Loopback.ToString();
                }
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
            }
            return myIP;
        }

        public static string FindDomainFromIp(string ipAddress)
        {
            string domainName = string.Empty;
            try
            {
                IPAddress addr = IPAddress.Parse(ipAddress);
                IPHostEntry entry = Dns.GetHostEntry(addr);
                domainName = entry.HostName;
            }
            catch (Exception)
            {
            }

            return domainName;
        }

        public static string GetConfigFilePath()
        {
            string pathToCopyConfigFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            pathToCopyConfigFile = Path.Combine(pathToCopyConfigFile, "Config");
            if (File.Exists(pathToCopyConfigFile))
            {
                return pathToCopyConfigFile;
            }
            else
            {
                return pathToCopyConfigFile + ".xml";
            }
        }

        public static byte[] CombineByteArray(byte[] previous, byte[] newone)
        {
            byte[] merged = null;
            try
            {
                if (previous == null)
                {
                    merged = new byte[newone.Length];

                    System.Buffer.BlockCopy(newone, 0, merged, 0, newone.Length);
                }
                else
                {
                    merged = new byte[previous.Length + newone.Length];

                    System.Buffer.BlockCopy(previous, 0, merged, 0, previous.Length);
                    System.Buffer.BlockCopy(newone, 0, merged, previous.Length, newone.Length);
                }
            }
            catch (Exception)
            {
                return merged;
            }
            return merged;
        }

        public static string GetWindowsUserName()
        {
            string userName = string.Empty;
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem"))
                {
                    using (ManagementObjectCollection collection = searcher.Get())
                    {
                        userName = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];
                    }
                }
            }
            catch (Exception)
            {

            }

            if (string.IsNullOrEmpty(userName))
            {
                userName = Environment.UserName;
            }

            return userName;
        }

        public static string Export(int registryhive, string registryPath, bool is64)
        {
            if (registryhive == 1)
            {
                registryPath = "HKLM\\" + registryPath;
            }
            else
            {
                string tempSID = GetWindowsUserSID();
                if (!string.IsNullOrEmpty(tempSID))
                {
                    registryPath = "HKU\\" + tempSID + "\\" + registryPath;
                }
                else
                {
                    return string.Empty;
                }
            }

            string exportPath = string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.Append("EXPORT  ");
            sb.Append("\"");
            sb.Append(registryPath);
            sb.Append("\" \"");

            if (is64)
            {
                exportPath = Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData) + @"\ProxyService\RegistryData\reg64.reg";
                sb.Append(exportPath);
                sb.Append("\"");
                sb.Append(" /reg:64");
            }
            else
            {
                exportPath = Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData) + @"\ProxyService\RegistryData\reg32.reg";
                sb.Append(exportPath);
                sb.Append("\"");
                sb.Append(" /reg:32");
            }
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(exportPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(exportPath));
                }
                if (File.Exists(exportPath))
                {
                    File.Delete(exportPath);
                }
            }
            catch (Exception ex)
            {
                //HTTPDataAnalyzer.Poll.//JobsSearcher.Logger.Error(ex);
            }
           
            Process proc = new Process();

            try
            {
                proc.StartInfo.FileName = "REG";
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.Arguments = sb.ToString();

                proc.Start();

                proc.WaitForExit();
            }
            catch (Exception ex)
            {
                //HTTPDataAnalyzer.Poll.//JobsSearcher.Logger.Error(ex);
                proc.Dispose();
            }
            return exportPath;
        }


        public static string GetWindowsUserSID()
        {
            string userName = string.Empty;
            string userSID = string.Empty;
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem"))
                {
                    using (ManagementObjectCollection collection = searcher.Get())
                    {
                        userName = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];
                    }
                }
            }
            catch (Exception ex)
            {
                //HTTPDataAnalyzer.Poll.//JobsSearcher.Logger.Error(ex);
                userName = Environment.UserName;
            }

            if (userName == null)
            {
                userName = Environment.UserName;
            }

            try
            {
                NTAccount f = new NTAccount(userName);
                SecurityIdentifier s = (SecurityIdentifier)f.Translate(typeof(SecurityIdentifier));
                userSID = s.ToString();
            }
            catch (Exception ex)
            {
                //HTTPDataAnalyzer.Poll.//JobsSearcher.Logger.Error(ex);
            }

            return userSID;
        }
    }
}
