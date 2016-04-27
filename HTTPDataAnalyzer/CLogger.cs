//using log4net;
//using log4net.Repository;
using System;
using System.IO;
using System.Linq;

namespace HTTPDataAnalyzer
{
    public class CLogger
    {
//        private ILog m_oLogger { get; set; }
//        private long m_threadIndex { get; set; }

//        public CLogger(string remoteUri, string userName, long nThreadIndex, bool isFromLua)
//        {
//            m_threadIndex = nThreadIndex;
//            CreateLogger(remoteUri, userName, isFromLua);
//        }

//        public void CreateLogger(string remoteUri, string userName, bool isFromLua)
//        {
//            if (isFromLua)
//            {
//                CreateLogger(remoteUri, userName);
//            }
//            else
//            {
//#if(DEBUG)
//                CreateLogger(remoteUri, userName);
//#else
//                m_oLogger = null;
//#endif
//            }
//        }

//        public static ILog CreateLoggers(string remoteUri, string userName)
//        {
//            ILog oLogger = null;
//            try
//            {
//                string repoName = String.Format("{0}Repository", userName);
//                ILoggerRepository repo = LogManager.CreateRepository(repoName);
               
//                ThreadContext.Properties["UserName"] = remoteUri;
//                ThreadContext.Properties["LogName"] = userName;
//                log4net.Config.XmlConfigurator.Configure(repo);

//                oLogger = LogManager.GetLogger(repoName, "AnalyzerLogger");

//            }
//            catch (Exception)
//            {
//                oLogger = null;
//            }
//            return oLogger;
//        }
//        public void CreateLogger(string remoteUri, string userName)
//        {
//            m_oLogger = null;
//            try
//            {
//                var invalidFileNameChars = Path.GetInvalidFileNameChars();
//                String strLogName = new string(remoteUri.Where(x => !invalidFileNameChars.Contains(x)).ToArray());

//                strLogName = strLogName.Substring(0, Math.Min(strLogName.Length, 80)) + DateTime.Now.ToString(ConstantVariables.DATE_FORMAT) + '{' + m_threadIndex + '}';
//                string repoName = String.Format("{0}Repository", strLogName);


//                if (userName == string.Empty || userName == null)
//                {
//                    userName = ConstantVariables.LUA_SCRIPTS_LOG_FOLDER;
//                }

//                ILoggerRepository repo = LogManager.CreateRepository(repoName);
//                ThreadContext.Properties["UserName"] = userName;
//                ThreadContext.Properties["LogName"] = strLogName;
//                log4net.Config.XmlConfigurator.Configure(repo);

//                // Set m_oLogger
//                m_oLogger = LogManager.GetLogger(repoName, "ProxyLogger");

//            }
//            catch (Exception ex)
//            {
//                //AnalyzerManager.Logger.Error(ex);
//                m_oLogger = null;
//            }
//        }

//        public void WriteLogException(String strMsg, Exception ex)
//        {
//            if (m_oLogger == null)
//            {
//                return;
//            }
//            m_oLogger.Error('{' + m_threadIndex + '}' + strMsg, ex);
//        }

//        public void WriteLogException(String strMsg)
//        {
//            if (m_oLogger == null)
//            {
//                return;
//            }
//            m_oLogger.Error('{' + m_threadIndex + '}' + strMsg);
//        }

//        public void WriteLogInfo(String strMsg)
//        {
//            if (m_oLogger == null)
//            {
//                return;
//            }
//            m_oLogger.Info('{' + m_threadIndex + '}' + strMsg);
//        }

//        public void WriteLogException(Exception ex)
//        {
//            if (m_oLogger == null)
//            {
//                return;
//            }
//            m_oLogger.Error(ex);
//        }

//        //Rajesh. Write Proxy Log
//        public void WriteProxyLog(String timestart, String timeEnd, String url, String status, String httpreq)
//        {
//            if (m_oLogger == null)
//            {
//                return;
//            }
//            m_oLogger.Info('{' + timestart + ", " + timeEnd + '}' + " [" + status + "]," + '{' + httpreq + '}' + ", " + url);
//        }

//        public void Cleanup()
//        {
//            try
//            {
//                if (m_oLogger != null)
//                {
//                    if (m_oLogger.Logger != null)
//                    {
//                        m_oLogger.Logger.Repository.Shutdown();
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                //AnalyzerManager.Logger.Error(ex);
//            }
//        }
    }
}
