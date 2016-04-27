using CertificateManagement;
////using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace AddRegisterEntriesInstaller
{
    public class RegistryEditor
    {
        [DllImport("wininet.dll")]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

        internal static string CurrentAssembleLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        //internal ILog Logger;

        internal static int INTERNET_OPTION_SETTINGS_CHANGED
        {
            get { return 39; }
        }

        internal static int INTERNET_OPTION_REFRESH
        {
            get { return 37; }
        }

        //public static RegistryEditor RegEditor = new RegistryEditor();

        public RegistryEditor()
        {
            try
            {
                //if (logger == null)
                //{
                //    ILog tempLogger = CLogger.CreateLogger();
                //    if (tempLogger != null)
                //    {
                //        Logger = tempLogger;
                //    }
                //}
                //else
                //{
                //    Logger = logger;
                //}
            }
            catch (Exception)
            {
            }

            FirefoxCertificateManager.GetFireFoxInstallationDirectory(this);
        }

        public void Start(string taskName, string overrideString = "")
        {
            switch (taskName)
            {
                case "install":
                    Install();
                    InstallOrUninstallRootCertificate(false);
                    InstallOrUninstallRootCertificate(true);
                    ProxyOverrider(overrideString);
                    FirefoxCertificateManager.Install(this);
                    Servicestarter.Start(this);
                    SpellInstall.NspInstall.InstallNSP();
                    break;
                case "uninstall":
                    Uninstall();
                    SpellInstall.NspInstall.UInstallNSP();
                    break;
                case "enable":
                    EnableOrDisableProxy(true);
                    break;
                case "disable":
                    EnableOrDisableProxy(false);
                    break;
                case "recheck":
                    FirefoxCertificateManager.Install(this);
                    if (Util.ModifiyConnString == null)
                    {
                        byte[] getConnString = GetDefaultConnSetting();
                        if (getConnString == null)
                        {
                            byte[] byteConnString = Util.DefaultConnString;
                            byteConnString = EditDefaultConnectionSettings.EditProxyOverride(byteConnString, string.Empty);
                            SetOrResetDialupSetting(true, byteConnString);
                        }
                        else
                        {
                            SetOrResetDialupSetting(true, getConnString);
                        }
                    }
                    else
                    {
                        SetOrResetDialupSetting(true, Util.ModifiyConnString);
                    }

                    break;
                case "proxyoverride":
                    ProxyOverrider(overrideString);
                    break;
                default:
                    break;
            }
        }

        public void Uninstall()
        {
            //Logger.Info("Enter");

            SetOrResetProxy(false, string.Empty);

            InstallOrUninstallRootCertificate(false);

            FirefoxCertificateManager.UnInstall(this);

            //Logger.Info("Exit");
        }

        private void InstallOrUninstallRootCertificate(bool isInstall)
        {
            //Logger.Info("Enter");

            try
            {
                if (isInstall)
                {
                    CertMaker.CreateAndExportRootCert(RegistryEditor.CurrentAssembleLocation + Path.DirectorySeparatorChar + FirefoxCertificateManager.FIREFOXTOOLS_FOLDERNAME + "\\proxyservice_root.crt");
                }
                else
                {
                    CertMaker.RemoveGeneratedCerts();
                }
            }
            catch (Exception ex)
            {

                //Logger.Error("Unable to create root certificate" + ex.Message);
            }

            //Logger.Info("Exit");
        }

        public void Install()
        {
            //Logger.Info("Enter");

            SetOrResetProxy(true, string.Empty);

            //Logger.Info("Exit");
        }

        public void ProxyOverrider(string overrideString)
        {

            var proxyEnable = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\Internet Settings", "ProxySettingsPerUser", null);
            if (proxyEnable != null && (int)proxyEnable == 0)
            {
                if (overrideString == null)
                {
                    overrideString = string.Empty;
                }
                SetOrResetProxy(true, overrideString);
                List<string> listOverrideString = overrideString.Split(new char[] { ';' }).ToList(); ;
                FirefoxCertificateManager.EditProxyOverride(this, listOverrideString);
            }
        }

        private void SetOrResetProxy(bool isSet, string overrideString)
        {
            //Logger.Info("Enter");

            byte[] byteConnString = Util.DefaultConnString;

            byteConnString = EditDefaultConnectionSettings.EditProxyOverride(byteConnString, overrideString);

            EnableOrDisableProxy(isSet);

            SetOrResetDialupSetting(isSet, byteConnString);

            try
            {
                RegistryKey localKey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                localKey = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\Connections", true);
                if (localKey != null)
                {
                    if (isSet)
                    {
                        localKey.SetValue("DefaultConnectionSettings", byteConnString);
                    }
                    else
                    {
                        localKey.DeleteValue("DefaultConnectionSettings");
                    }
                }
                localKey.Close();
            }
            catch (Exception ex)
            {
                //Logger.Error(ex);
            }

            try
            {
                RegistryKey localKey32 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                localKey32 = localKey32.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\Connections", true);
                if (localKey32 != null)
                {
                    if (isSet)
                    {
                        localKey32.SetValue("DefaultConnectionSettings", byteConnString);
                    }
                    else
                    {
                        localKey32.DeleteValue("DefaultConnectionSettings");
                    }
                }
                localKey32.Close();
            }
            catch (Exception ex)
            {
                //Logger.Error(ex);
            }

            // These lines implement the Interface in the beginning of program 
            // They cause the OS to refresh the settings, causing IP to realy update
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_SETTINGS_CHANGED, IntPtr.Zero, 0);
            InternetSetOption(IntPtr.Zero, INTERNET_OPTION_REFRESH, IntPtr.Zero, 0);
            Util.ModifiyConnString = byteConnString;

            //Logger.Info("Exit");
        }

        private void EnableOrDisableProxy(bool isSet)
        {
            if (isSet)
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\Internet Settings", "ProxySettingsPerUser", (object)0);
                FirefoxCertificateManager.EnableOrDisablePorxy(this, true);
            }
            else
            {
                Microsoft.Win32.Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\Internet Settings", "ProxySettingsPerUser", (object)1);
                FirefoxCertificateManager.EnableOrDisablePorxy(this, false);
            }
        }

        public void SetOrResetDialupSetting(bool isSet, byte[] byteConnString)
        {
            //Logger.Info("Enter");

            Dialup.RASCONN[] rsaObj = Dialup.GetRasConnections();
            if (rsaObj.Count() == 0)
                return;

            try
            {
                RegistryKey localKey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                localKey = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\Connections", true);
                if (localKey != null)
                {
                    if (isSet)
                    {
                        for (int i = 0; i < rsaObj.Count(); i++)
                        {
                            localKey.SetValue(rsaObj[i].szEntryName, byteConnString);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < rsaObj.Count(); i++)
                        {
                            localKey.DeleteValue(rsaObj[i].szEntryName);
                        }
                    }
                }
                localKey.Close();
            }
            catch (Exception ex)
            {
                //Logger.Error(ex);
            }

            try
            {
                RegistryKey localKey32 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                localKey32 = localKey32.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\Connections", true);
                if (localKey32 != null)
                {
                    if (isSet)
                    {
                        for (int i = 0; i < rsaObj.Count(); i++)
                        {
                            localKey32.SetValue(rsaObj[i].szEntryName, byteConnString);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < rsaObj.Count(); i++)
                        {
                            localKey32.DeleteValue(rsaObj[i].szEntryName);
                        }
                    }

                }
                localKey32.Close();
            }
            catch (Exception ex)
            {
                //Logger.Error(ex);
            }
            //Logger.Info("Exit");
        }

        public byte[] GetDefaultConnSetting()
        {
            byte[] byteConnString = null;
            try
            {
                try
                {
                    RegistryKey localKey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                    localKey = localKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\Connections", true);

                    byteConnString = (byte[])localKey.GetValue("DefaultConnectionSettings");

                    localKey.Close();
                }
                catch (Exception ex)
                {
                    //Logger.Error(ex);
                }

                if (byteConnString == null)
                {

                    RegistryKey localKey32 = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                    localKey32 = localKey32.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\Connections", true);
                    byteConnString = (byte[])localKey32.GetValue("DefaultConnectionSettings");
                    localKey32.Close();
                }
            }
            catch (Exception ex)
            {
                //Logger.Error(ex);
            }
            return byteConnString;
        }
    }
}
