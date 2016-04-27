//using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HTTPDataAnalyzer.Poll
{
    class SystemInfoUpdater
    {
        //public static ILog Logger;
        public static void Start()
        {
            //Logger.Info("Enter");

            while (true)
            {
                UpdateHostInfo();
                Thread.Sleep(60000);
            }
        }

        static SystemInfoUpdater()
        {
            try
            {
                //ILog tempLogger = CLogger.CreateLoggers("AnalyzerLogs", "SystemInfoUpdaterLogger");
                //if (tempLogger != null)
                //{
                //    Logger = tempLogger;
                //}
            }
            catch (Exception)
            {
            }
        }

        private static void UpdateHostInfo()
        {
            //Logger.Info("Enter");

            byte[] bodyBytes = Registration.ConfigFinder.GetConfig(false);
            if (TestTCPClient.TestConfig.TestCheck)
            {
                TestTCPClient.UpdateClientInfo("01", bodyBytes);
            }
            else
            {
                TCPClients.UpdateClientInfo("01", bodyBytes);
            }

            //Logger.Info("Exit");
        }
    }
}
