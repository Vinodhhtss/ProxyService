//using log4net;
using System;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace HTTPDataAnalyzer.Registration
{
    public class ClientRegistrar
    {
        //public static ILog Logger;
        public static void Start()
        {
            if (ConfigHandler.Config.HostInfoes != null && !string.IsNullOrEmpty(ConfigHandler.Config.HostInfoes.HostID))
            {
                ConfigHandler.HostInfoes = ConfigHandler.Config.HostInfoes;
                ConfigHandler.ServerDetail = ConfigHandler.Config.ServerDetails;
                ConfigHandler.AgentDetail = ConfigHandler.Config.AgentInstaller;
                TestTCPClient.ReadHostIds();
            }
            else
            {
                RegisterClient();
            }
            AnalyzerManager.Start();
        }

        static ClientRegistrar()
        {
            try
            {
                //ILog tempLogger = CLogger.CreateLoggers("AnalyzerLogs", "ClientRegistrarLogger");
                //if (tempLogger != null)
                //{
                //    Logger = tempLogger;
                //}
            }
            catch
            {

            }
        }

        private static bool CheckAndGetRegisteredClientID()
        {
            //Logger.Info("Enter");

            bool exists = false;
            try
            {
                XElement xmlDoc = XElement.Parse(ConfigHandler.ReadConfigFile());

                exists = xmlDoc.Elements("ServerCofig").Elements("clientid").Any();
                XElement ServerCofig = xmlDoc.Element("ServerCofig");
                if (exists)
                {
                    if (string.IsNullOrEmpty(xmlDoc.Element("ServerCofig").Element("clientid").Value))
                    {
                        exists = false;
                    }
                }
            }
            catch (Exception ex)
            {
                //Logger.Error(ex);
            }

            //Logger.Info("Exit");
            return exists;
        }

        private static void RegisterClient()
        {
            //Logger.Info("Enter");

           // CertHandler.RemoveConsoleServerCertificate();
            while (true)
            {
                if (TCPClients.GetCertificate("00"))
                {
                    break;
                }
                Thread.Sleep(TimeSpan.FromSeconds(30));
            }

            byte[] bodyBytes = Registration.ConfigFinder.GetConfig(true);
            while (true)
            {
                if (TestTCPClient.TestConfig.TestCheck)
                {
                    if (TestTCPClient.RegisterClientWithServer("0", bodyBytes))
                    {
                        TestTCPClient.GetGuids();
                        TestTCPClient.ReadHostIds();
                        break;
                    }
                }
                else
                {
                    if (TCPClients.RegisterClientWithServer("0", bodyBytes))
                    {
                        break;
                    }
                }

                Thread.Sleep(TimeSpan.FromSeconds(30));
            }

            //Logger.Info("Exit");
        }
    }
}
