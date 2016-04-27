//using log4net;
using System;
using System.Data;
using System.Threading;

namespace HTTPDataAnalyzer.StoreAndForward
{
    public class StoredAndForward
    {
        public static ProxyDbs.ProxyDb DBHandle = new ProxyDbs.ProxyDb();
        //public static ILog Logger;
        public static void Start()
        {
            //Logger.Info("Enter");
            while (true)
            {
                // while (!ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(StartStoredAndForward), null)) ;
                StartStoredAndForward(null);
                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }

        static StoredAndForward()
        {
            try
            {
                //ILog tempLogger = CLogger.CreateLoggers("AnalyzerLogs", "StoredAndForwardLogger");
                //if (tempLogger != null)
                //{
                //    Logger = tempLogger;
                //}
            }
            catch (Exception)
            {
            }
        }

        private static void StartStoredAndForward(object obj)
        {
            //Logger.Info("Enter");
            try
            {
                string tempQueryString = "select * from alert_failed";
                DataTable dt = AnalyzerManager.ProxydbObj.GetTableFromDB(tempQueryString, "AlertDetails");
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {

                        if (TestTCPClient.TestConfig.TestCheck)
                        {
                            if (TestTCPClient.SendAlertMessageToServer("2", (byte[])row[1], false))
                            {
                                string command = "delete from  alert_failed where dbid = " + Convert.ToInt32(row[0]);
                                ProxyDbs.ProxyDb.DeleteRowFromTable(command);
                            }
                        }
                        else
                        {
                            if (TCPClients.SendAlertMessageToServer("2", (byte[])row[1], false))
                            {
                                string command = "delete from  alert_failed where dbid = " + Convert.ToInt32(row[0]);
                                ProxyDbs.ProxyDb.DeleteRowFromTable(command);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Logger.Error(ex);
            }

            try
            {
                string tempQueryString = "select * from lazy_failed";
                DataTable dt = AnalyzerManager.ProxydbObj.GetTableFromDB(tempQueryString, "LazyDetails");
                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        if (TestTCPClient.TestConfig.TestCheck)
                        {
                            if (TestTCPClient.SendLazyPacketsToServer("1", (byte[])row[1], false))
                            {
                                string command = "delete from  lazy_failed where dbid = " + Convert.ToInt32(row[0]);
                                ProxyDbs.ProxyDb.DeleteRowFromTable(command);
                            }
                        }
                        else
                        {
                            if (TCPClients.SendLazyPacketsToServer("1", (byte[])row[1], false))
                            {
                                string command = "delete from  lazy_failed where dbid = " + Convert.ToInt32(row[0]);
                                ProxyDbs.ProxyDb.DeleteRowFromTable(command);
                            }
                        }
                    }
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
