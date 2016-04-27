using AddRegisterEntriesInstaller;
//using log4net;
using System;
using System.Text;
using System.Threading;

namespace HTTPDataAnalyzer.Poll
{
    public class ConfigurationDetector
    {
        //public static ILog Logger;
        public static ConfigParameters PreviousConfig = null;
        public static void Start()
        {
            //Logger.Info("Enter");
            PreviousConfig = ConfigHandler.Config;
            while (true)
            {
                //while (!ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(SearchConfigurationChange), null)) ;
                SearchConfigurationChange(null);
                UpdateHostConfig();
                Thread.Sleep(ConfigHandler.Config.Policies.HeartBeatInterval);
            }
        }

        static ConfigurationDetector()
        {
            try
            {
                //ILog tempLogger = CLogger.CreateLoggers("AnalyzerLogs", "ConfigurationDetectorLogger");
                //if (tempLogger != null)
                //{
                //    Logger = tempLogger;
                //}
            }
            catch (Exception)
            {
            }
        }

        private static void UpdateHostConfig()
        {
            //Logger.Info("Enter");

            AddRegisterEntriesInstaller.RegistryEditor regEditor = new AddRegisterEntriesInstaller.RegistryEditor();
            if (ConfigHandler.Config.Policies.IsProxyEnabled)
            {
                if (PreviousConfig.Policies.IsProxyEnabled != ConfigHandler.Config.Policies.IsProxyEnabled)
                {
                    regEditor.Start("enable");
                }
                regEditor.Start("recheck");
                if (PreviousConfig.ByPassDetails.ByPassString != ConfigHandler.Config.ByPassDetails.ByPassString)
                {
                    regEditor.Start("proxyoverride", ConfigHandler.Config.ByPassDetails.ByPassString);
                }
            }
            else
            {
                if (PreviousConfig.Policies.IsProxyEnabled != ConfigHandler.Config.Policies.IsProxyEnabled)
                {
                    regEditor.Start("disable");
                }
            }

            PreviousConfig = ConfigHandler.Config;

            //Logger.Info("Exit");
        }

        public static void SearchConfigurationChange(object obj)
        {
            //Logger.Info("Enter");

            try
            {
                byte[] output = Encoding.ASCII.GetBytes("");
                if (TestTCPClient.TestConfig.TestCheck)
                {
                    TestTCPClient.SearchConfigChangeInServer("4", output);
                }
                else
                {
                    TCPClients.SearchConfigChangeInServer("4", output);
                }
            }
            catch (Exception ex)
            {
                //Logger.Error(ex);
            }

            //Logger.Info("Exit");
        }
    }
}
