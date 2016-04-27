using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace HTTPDataAnalyzer.Registration
{
    public class Browser
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
        [JsonProperty("proxyenabled")]
        public int ProxyEnabled { get; set; }
    }

    internal class BrowsersDetector
    {
        public static Dictionary<string, Browser> GetBrowsers()
        {
            var browsers = new Dictionary<string, Browser>();
            try
            {
                RegistryKey browserKeys = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Clients\StartMenuInternet");
                if (browserKeys == null)
                {
                    browserKeys = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet");
                }
                GetBrowsers(browserKeys, browsers);
                List<string> userProfiles = winaudits.RegistryUtil.GetUserProfiles();
                foreach (var item in userProfiles)
                {
                    browserKeys = Registry.Users.OpenSubKey(item + "\\SOFTWARE\\Clients\\StartMenuInternet");
                    if (browserKeys != null)
                    {
                        GetBrowsers(browserKeys, browsers);
                    }
                }
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
            }

            return browsers;
        }

        public static Dictionary<string, Browser> GetBrowsers(RegistryKey browserKeys, Dictionary<string, Browser> browsers)
        {
            string[] browserNames = browserKeys.GetSubKeyNames();
            for (int i = 0; i < browserNames.Length; i++)
            {
                try
                {
                    Browser browser = new Browser();
                    browser.ProxyEnabled = 1;
                    RegistryKey browserKey = browserKeys.OpenSubKey(browserNames[i]);
                    browser.Name = ((string)browserKey.GetValue(null)).ToLower(); ;
                    RegistryKey browserKeyPath = browserKey.OpenSubKey(@"shell\open\command");
                    string path = (string)browserKeyPath.GetValue(null).ToString().Replace("\"", string.Empty);
                    if (path != null && File.Exists(path))
                    {
                        browser.Version = FileVersionInfo.GetVersionInfo(path).FileVersion;
                    }
                    else
                    {
                        browser.Version = "unknown";
                    }

                    if (File.Exists(path))
                    {
                        if (!browsers.ContainsKey(browser.Name))
                        {
                            browsers.Add(browser.Name, browser);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Registration.ClientRegistrar.Logger.Error(ex);
                }
            }
            return browsers;
        }
    }
}

