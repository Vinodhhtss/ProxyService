//using log4net;
//using log4net.Repository;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace HTTPProxyServer
{
    public class CLogger
    {
//        public ILog Logger { get; set; }
//        public long ThreadIndex { get; set; }

//        public CLogger(string remoteUri, string userName, long nThreadIndex, bool isFromLua)
//        {
//            ThreadIndex = nThreadIndex;
//            CreateLogger(remoteUri, userName, isFromLua);
//        }

//        private void CreateLogger(string remoteUri, string userName, bool isFromLua)
//        {
//            if (isFromLua)
//            {
//                CreateLogger(remoteUri, userName);
//            }
//            else
//            {
//#if(DEBUG)
//                CreateLogger(remoteUri, userName);
//                //#else
//                //              m_oLogger = null;
//#endif
//            }
//        }

//        private void CreateLogger(string remoteUri, string userName)
//        {
//            Logger = null;
//            try
//            {
//                var invalidFileNameChars = Path.GetInvalidFileNameChars();
//                String strLogName = new string(remoteUri.Where(x => !invalidFileNameChars.Contains(x)).ToArray());


//                strLogName = strLogName.Substring(0, Math.Min(strLogName.Length, 80)) + DateTime.Now.ToString(ConstantVariables.DATE_FORMAT) + "{" + ThreadIndex + "}";
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
//                Logger = LogManager.GetLogger(repoName, "ProxyLogger");

//            }
//            catch (Exception ex)
//            {
//                Debug.Write(ex.Message + " " + ex.StackTrace);
//                Logger = null;
//            }
//        }

//        //public void WriteLogException(String strMsg, Exception ex)
//        //{
//        //    if (Logger == null)
//        //    {
//        //        return;
//        //    }
//        //    //Logger.Error('{' + m_threadIndex + '}' + strMsg, ex);
//        //}

//        //public void WriteLogException(String strMsg)
//        //{
//        //    if (Logger == null)
//        //    {
//        //        return;
//        //    }
//        //    //Logger.Error('{' + m_threadIndex + '}' + strMsg);
//        //}

//        //public void WriteLogException(Exception ex)
//        //{
//        //    if (Logger == null)
//        //    {
//        //        return;
//        //    }
//        //    //Logger.Error(ex);
//        //}

//        //public void WriteLogInfo(String strMsg)
//        //{
//        //    if (Logger == null)
//        //    {
//        //        return;
//        //    }
//        //    //Logger.Info('{' + m_threadIndex + '}' + strMsg);
//        //}

//        //Rajesh. Write Proxy Log
//        //public void WriteProxyLog(String timestart, String timeEnd, String url, String status, String httpreq)
//        //{
//        //    if (Logger == null)
//        //    {
//        //        return;
//        //    }
//        //    //Logger.Info('{' + timestart + ", " + timeEnd + '}' + " [" + status + "]," + '{' + httpreq + '}' + ", " + url);
//        //}

//        public void Cleanup()
//        {
//            try
//            {
//                if (Logger != null)
//                {
//                    if (Logger.Logger != null)
//                    {
//                        Logger.Logger.Repository.Shutdown();
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                //TCPClientProcessor.Proxylog.WriteLogException(ex);
//            }
//        }
    }
}
