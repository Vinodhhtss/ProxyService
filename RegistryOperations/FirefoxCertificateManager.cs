using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;

namespace AddRegisterEntriesInstaller
{
    internal class FirefoxCertificateManager
    {
        //Replace "Local CA" with the name of your local certificate authority.
        private static string LocalCertificateAuthorityName
        {
            get { return "Proxyservice"; }
        }

        //Replace "certificate_file.crt" with the name of your certificate file.
        private static string CertificateFileName
        {
            get { return "proxyservice_root.crt"; }
        }

        //Set appropriate trust for the certificate authority by editing "CT,c,C".
        //Refer to https://developer.mozilla.org/en-US/docs/Mozilla/Projects/NSS/tools/NSS_Tools_certutil for more information about the certutil tool.
        private static string TrustAttributes
        {
            get { return "CT,c,C"; }
        }

        internal static string FIREFOXTOOLS_FOLDERNAME = "FirefoxTools";
        private static string FireFoxVersion = string.Empty;
        private static string FirefoxInstallationDirectory = string.Empty;

        private static List<string> FireFoxCertAddProfiles = new List<string>();

        public static void Install(RegistryEditor regEditor)
        {
            ////regEditor.Logger.Info("Enter");

            if (!string.IsNullOrEmpty(FireFoxVersion) && !string.IsNullOrEmpty(FirefoxInstallationDirectory))
            {
                //Firefox installed. so disable proxy setings.
                try
                {
                    string tempPath = UnHideFiles(Path.Combine(FirefoxInstallationDirectory, "mozilla.cfg"), regEditor);
                    if (!string.IsNullOrEmpty(tempPath))
                    {
                        File.Copy(Path.Combine(RegistryEditor.CurrentAssembleLocation, "mozilla.cfg"), tempPath, true);
                    }
                    tempPath = UnHideFiles(Path.Combine(FirefoxInstallationDirectory, @"defaults\pref\local-settings.js"), regEditor);
                    if (!string.IsNullOrEmpty(tempPath))
                    {
                        File.Copy(Path.Combine(RegistryEditor.CurrentAssembleLocation, "local-settings.js"), tempPath, true);
                    }
                }
                catch (Exception ex)
                {
                    //regEditor.Logger.Error(ex);
                }
            }

            InstallCert(regEditor);

            ////regEditor.Logger.Info("Exit");
        }

        public static void InstallCert(RegistryEditor regEditor)
        {
            ////regEditor.Logger.Info("Enter");
            //Check to see if Firefox is installed. If Firefox is not installed, the rest of the code is not run.
            if (!string.IsNullOrEmpty(FirefoxInstallationDirectory))
            {
                foreach (string strFirefoxProfilesDir in GetAllWindowsUsersAppData(regEditor))
                {
                    //Find the list of Firefox profile folders for the currect user.
                    DirectoryInfo directory = new DirectoryInfo(strFirefoxProfilesDir);
                    //Update cert.db in each of the profile folders.
                    if (!Directory.Exists(strFirefoxProfilesDir))
                    {
                        continue;
                    }
                    try
                    {
                        foreach (DirectoryInfo FireFoxProfile in directory.GetDirectories())
                        {
                            try
                            {
                                if (!File.Exists(FireFoxProfile.FullName + @"\cert8.db") || FireFoxCertAddProfiles.Contains(FireFoxProfile.FullName))
                                    continue;

                                //Create a backup of the old cert8.db file. This line is optional.
                                File.Copy(FireFoxProfile.FullName + @"\cert8.db", FireFoxProfile.FullName + @"\cert8.db.old", true);

                                //Add the local CA certificate to cert8.db and assign appropriate trust levels.
                                //This calls the certutil tool that was copied to the installation directory.

                                String strProgramName = RegistryEditor.CurrentAssembleLocation + Path.DirectorySeparatorChar + FIREFOXTOOLS_FOLDERNAME + @"\certutil.exe ";

                                String strArguments = @" -A -n " + LocalCertificateAuthorityName + " -i " + (char)34 + RegistryEditor.CurrentAssembleLocation + Path.DirectorySeparatorChar +
                                    FIREFOXTOOLS_FOLDERNAME + Path.DirectorySeparatorChar + CertificateFileName + (char)34 + " -t " + TrustAttributes + " -d " + (char)34 + FireFoxProfile.FullName + (char)34;
                                SilentProcessExecuter(strProgramName, strArguments);
                                FireFoxCertAddProfiles.Add(FireFoxProfile.FullName);
                            }
                            catch (Exception ex)
                            {
                                //regEditor.Logger.Error(ex);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //regEditor.Logger.Error(ex);
                    }
                }
            }

            ////regEditor.Logger.Info("Exit");
        }

        public static void EditProxyOverride(RegistryEditor regEditor, IList<string> byPassList)
        {
            ////regEditor.Logger.Info("Enter");

            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in byPassList)
                {
                    sb.Append(item);
                    sb.Append(',');
                }

                if (!File.Exists(FirefoxInstallationDirectory))
                {
                    return;
                }

                string configFilePath = Path.Combine(FirefoxInstallationDirectory, "mozilla.cfg");
                string line = string.Empty;

                string[] arrLine = File.ReadAllLines(configFilePath);

                bool proxyOverrideFound = false;

                string no_proxies = "pref(\"network.proxy.no_proxies_on\"," + "\"" + sb.ToString().TrimEnd(new char[] { ',' }) + "\"" + ")";
                for (int i = 0; i < arrLine.Length; i++)
                {
                    if (arrLine[i].Contains("network.proxy.no_proxies_on"))
                    {
                        arrLine[i] = no_proxies;
                        proxyOverrideFound = true;
                        break;
                    }
                }

                List<string> lines = new List<string>();
                lines.AddRange(arrLine.ToList());
                if (!proxyOverrideFound)
                {
                    lines.Add(no_proxies);
                }
                File.WriteAllLines(configFilePath, lines);
            }
            catch (Exception ex)
            {
                //regEditor.Logger.Error(ex);
            }

            ////regEditor.Logger.Info("Exit");
        }

        public static void EnableOrDisablePorxy(RegistryEditor regEditor, bool isEnable)
        {
            ////regEditor.Logger.Info("Enter");

            if (!File.Exists(FirefoxInstallationDirectory))
            {
                return;
            }
            string configFilePath = Path.Combine(FirefoxInstallationDirectory, "mozilla.cfg");

            string line = string.Empty;

            string[] arrLine = File.ReadAllLines(configFilePath);

            bool proxyTypeFound = false;

            string proxy_type = string.Empty;
            if (isEnable)
            {
                proxy_type = "lockPref(\"network.proxy.type\", 1)";
            }
            else
            {
                proxy_type = "pref(\"network.proxy.type\", 0)";
            }
            for (int i = 0; i < arrLine.Length; i++)
            {
                if (arrLine[i].Contains("network.proxy.type"))
                {
                    arrLine[i] = proxy_type;
                    proxyTypeFound = true;
                    break;
                }
            }
            List<string> lines = new List<string>();
            lines.AddRange(arrLine.ToList());
            if (!proxyTypeFound)
            {
                lines.Add(proxy_type);
            }
            File.WriteAllLines(configFilePath, lines);

            ////regEditor.Logger.Info("Exit");
        }

        private static string UnHideFiles(string path, RegistryEditor regEditor)
        {
            ////regEditor.Logger.Info("Enter");
            if (!File.Exists(path))
            {
                return path;
            }

            FileAttributes attributes = File.GetAttributes(path);

            if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                attributes = RemoveAttribute(attributes, FileAttributes.Hidden);
                File.SetAttributes(path, attributes);
            }
            ////regEditor.Logger.Info("Exit");
            return path;
        }

        private static FileAttributes RemoveAttribute(FileAttributes attributes, FileAttributes attributesToRemove)
        {
            return attributes & ~attributesToRemove;
        }

        private static List<string> GetAllWindowsUsersAppData(RegistryEditor regEditor)
        {
            ////regEditor.Logger.Info("Enter");
            string regKeyFolders = @"HKEY_USERS\<SID>\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders";
            string regValueAppData = @"AppData";
            string[] keys = Registry.Users.GetSubKeyNames();
            List<String> paths = new List<String>();

            foreach (string sid in keys)
            {
                string appDataPath = Registry.GetValue(regKeyFolders.Replace("<SID>", sid), regValueAppData, null) as string;
                if (appDataPath != null)
                {
                    paths.Add(appDataPath + @"\Mozilla\Firefox\Profiles");
                }
            }
            ////regEditor.Logger.Info("Exit");
            return paths;
        }

        public static void UnInstall(RegistryEditor regEditor)
        {
            ////regEditor.Logger.Info("Enter");

            if (!string.IsNullOrEmpty(FireFoxVersion) && !string.IsNullOrEmpty(FirefoxInstallationDirectory))
            {
                File.Delete(Path.Combine(FirefoxInstallationDirectory, "mozilla.cfg"));
                File.Delete(Path.Combine(FirefoxInstallationDirectory, @"defaults\pref\local-settings.js"));
            }

            //Check to see if Firefox is installed. If Firefox is not installed, the rest of the code is not run.
            if (!string.IsNullOrEmpty(FirefoxInstallationDirectory))
            {
                foreach (string strFirefoxProfilesDir in GetAllWindowsUsersAppData(regEditor))
                {
                    //Find the list of Firefox profile folders for the currect user.
                    DirectoryInfo directory = new DirectoryInfo(strFirefoxProfilesDir);

                    //Update cert.db in each of the profile folders.
                    foreach (DirectoryInfo FireFoxProfile in directory.GetDirectories())
                    {
                        try
                        {
                            if (!File.Exists(FireFoxProfile.FullName + @"\cert8.db"))
                                continue;

                            //Create a backup of the old cert8.db file. This line is optional.
                            File.Copy(FireFoxProfile.FullName + @"\cert8.db", FireFoxProfile.FullName + @"\cert8.db.old", true);

                            //Add the local CA certificate to cert8.db and assign appropriate trust levels.
                            //This calls the certutil tool that was copied to the installation directory.

                            String strProgramName = RegistryEditor.CurrentAssembleLocation + Path.DirectorySeparatorChar + FIREFOXTOOLS_FOLDERNAME + @"\certutil.exe ";

                            String strArguments = " -n " + LocalCertificateAuthorityName + " -D " + (char)34 + RegistryEditor.CurrentAssembleLocation + Path.DirectorySeparatorChar + FIREFOXTOOLS_FOLDERNAME + Path.DirectorySeparatorChar
                                    + CertificateFileName + (char)34 + " -d " + (char)34 + FireFoxProfile.FullName + (char)34;

                            SilentProcessExecuter(strProgramName, strArguments);
                        }
                        catch (Exception ex)
                        {
                            //regEditor.Logger.Error(ex);
                        }
                    }
                }
            }

            ////regEditor.Logger.Info("Exit");
        }

        private static void SilentProcessExecuter(String programName, String arguments)
        {
            Process process = new Process();
            process.StartInfo.FileName = programName;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
        }

        public static void GetFireFoxInstallationDirectory(RegistryEditor regEditor)
        {
            ////regEditor.Logger.Info("Enter");
            try
            {
                if (Environment.Is64BitOperatingSystem)
                {
                    FireFoxVersion = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Mozilla\Mozilla Firefox\", "CurrentVersion", string.Empty).ToString();
                    if (string.IsNullOrEmpty(FireFoxVersion))
                    {
                        FireFoxVersion = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Mozilla\Mozilla Firefox ESR\", "CurrentVersion", string.Empty).ToString();
                    }
                    FirefoxInstallationDirectory = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Mozilla\Mozilla Firefox\" + FireFoxVersion + @"\Main", "Install Directory", string.Empty).ToString();

                }
                else
                {
                    FireFoxVersion = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Mozilla\Mozilla Firefox\", "CurrentVersion", string.Empty).ToString();
                    if (string.IsNullOrEmpty(FireFoxVersion))
                    {
                        FireFoxVersion = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Mozilla\Mozilla Firefox ESR\", "CurrentVersion", string.Empty).ToString();
                    }
                    FirefoxInstallationDirectory = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Mozilla\Mozilla Firefox\" + FireFoxVersion + @"\Main", "Install Directory", string.Empty).ToString();
                }

            }
            catch (Exception ex)
            {
                //regEditor.Logger.Error(ex);
                FireFoxVersion = string.Empty;
                FirefoxInstallationDirectory = string.Empty;
            }
            ////regEditor.Logger.Info("Exit");
        }
    }
}