//using log4net;
using System;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace HTTPDataAnalyzer
{
    public class AnalyzerManager
    {
        private static Thread m_RegisterThread = null;
        private static ProxyDbs.ProxyDb m_proxydb;

        //public static ILog Logger;

        public static ProxyDbs.ProxyDb ProxydbObj
        {
            get { return AnalyzerManager.m_proxydb; }
            set { AnalyzerManager.m_proxydb = value; }
        }   

        private static Thread m_AlertAnalyzerThread;
        private static Thread m_JobsSearcherThread;
        private static Thread m_ConfigurationDetectorThread;
        private static Thread m_StoreAndForwardThread;
        private static Thread m_SystemInfoUpdaterThread;

        public static AnalyzerManager Analyzer { get; set; }


        public AnalyzerManager()
        {
            m_proxydb = new ProxyDbs.ProxyDb(false);
            DBManager.Start();
            winaudits.DBManager.Start();

            try
            {
                string config = ConfigHandler.ReadConfigFile();
                ConfigHandler.Config = ConfigHandler.DeSerializeToConfigParams(config);
                SaveProxyConfig();
            }
            catch (Exception ex)
            {
                //Logger.Error(ex.Message);
            }

            //temp code need to be handled
            try
            {
                //ILog tempLogger = CLogger.CreateLoggers("AnalyzerLogs", "AnalyzerLog");
                //if (tempLogger != null)
                //{
                //    Logger = tempLogger;
                //}
            }
            catch (Exception ex)
            {
                //Logger.Error(ex.Message);
            }

            try
            {
                if (ConfigHandler.Config.ByPassDetails != null)
                {
                    AddRegisterEntriesInstaller.RegistryEditor regEditor = new AddRegisterEntriesInstaller.RegistryEditor();
                    regEditor.Start("proxyoverride", ConfigHandler.Config.ByPassDetails.ByPassString);
                }
            }
            catch (Exception ex)
            {
                //Logger.Error(ex.Message);
            }
            m_RegisterThread = new Thread(Registration.ClientRegistrar.Start);
            m_RegisterThread.IsBackground = true;
            m_RegisterThread.Start();
        }

        public static void SaveProxyConfig()
        {
            string tempPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            tempPath = Path.Combine(tempPath, "ProxyConfig.xml");

            File.AppendAllText(tempPath, SerializeToXML(ConfigHandler.Config.UpStreamProxies));
        }

        public static string SerializeToXML(UpStreamProxy upStreamProxy)
        {
            string toXML = string.Empty;
            XmlSerializer xsSubmit = new XmlSerializer(typeof(UpStreamProxy));
            using (StringWriter sww = new StringWriter())
            using (XmlWriter writer = XmlWriter.Create(sww))
            {
                xsSubmit.Serialize(writer, upStreamProxy);
                toXML = sww.ToString();

            }

            return toXML;
        }

        public static void Start()
        {
            Lazy.LazyManager.Starter();

            m_AlertAnalyzerThread = new Thread(new ThreadStart(StartPipeServer.StartServer));
            m_AlertAnalyzerThread.IsBackground = true;
            m_AlertAnalyzerThread.Start();

            m_JobsSearcherThread = new Thread(new ThreadStart(Poll.JobsSearcher.Start));
            m_JobsSearcherThread.IsBackground = true;
            m_JobsSearcherThread.Start();

            m_ConfigurationDetectorThread = new Thread(new ThreadStart(Poll.ConfigurationDetector.Start));
            m_ConfigurationDetectorThread.IsBackground = true;
            m_ConfigurationDetectorThread.Start();

            m_SystemInfoUpdaterThread = new Thread(new ThreadStart(Poll.SystemInfoUpdater.Start));
            m_SystemInfoUpdaterThread.IsBackground = true;
            m_SystemInfoUpdaterThread.Start();

            m_StoreAndForwardThread = new Thread(new ThreadStart(StoreAndForward.StoredAndForward.Start));
            m_StoreAndForwardThread.IsBackground = true;
            m_StoreAndForwardThread.Start();
        }

        public bool Stop()
        {
            try
            {
                Lazy.LazyManager.Stop();
                StartPipeServer.StopServer();
                if (m_AlertAnalyzerThread != null)
                {
                    m_AlertAnalyzerThread.Abort();
                }
                if (m_JobsSearcherThread != null)
                {
                    m_JobsSearcherThread.Abort();
                }
                if (m_AlertAnalyzerThread != null)
                {
                    m_ConfigurationDetectorThread.Abort();
                }
                if (m_StoreAndForwardThread != null)
                {
                    m_StoreAndForwardThread.Abort();
                }
                if (m_AlertAnalyzerThread != null)
                {
                    m_AlertAnalyzerThread.Join();
                }
                if (m_JobsSearcherThread != null)
                {
                    m_JobsSearcherThread.Join();
                }
                if (m_ConfigurationDetectorThread != null)
                {
                    m_ConfigurationDetectorThread.Join();
                }
                if (m_StoreAndForwardThread != null)
                {
                    m_StoreAndForwardThread.Join();
                }
            }
            catch (Exception ex)
            {
                //Logger.Error(ex.Message);
                return false;
            }

            return true;
        }
    }
}
