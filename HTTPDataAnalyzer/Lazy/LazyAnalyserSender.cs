//using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace HTTPDataAnalyzer.Lazy
{
    class LazyAnalyserSender
    {
        //private static int m_PrevLastRecordID = 0;

        private static object m_CountLock = new object();
        public Queue<SessionHandler> CurrentBuffer = new Queue<SessionHandler>();

        //public static ILog Logger;

        static LazyAnalyserSender()
        {
            //ILog tempLogger = CLogger.CreateLoggers("AnalyzerLogs", "LazyAnalyzerSenderLogger");
            //if (tempLogger != null)
            //{
            //    Logger = tempLogger;
            //}
        }

        public void Analyse()
        {
            //Logger.Info("Enter");

            try
            {
                ReadDataFromDB();
            }
            catch (Exception ex)
            {
                //Logger.Info(ex);
            }

            //Logger.Info("Exit");
        }

        private void ReadDataFromDB()
        {
            //Logger.Info("Enter");

            DataTable dt = null;
            lock (m_CountLock)
            {
                string tempQueryString = "select * from Request INNER JOIN Response ON  Response.request_id = Request.dbid where  Response.lazy_status = 2;";
                try
                {
                    dt = AnalyzerManager.ProxydbObj.GetTableFromDB(tempQueryString, "PacketDetails");
                }
                catch (Exception ex)
                {
                    //Logger.Error(ex);
                }

                if (dt == null || dt.Rows.Count <= 0)
                {
                    return;
                }

                //int nCurrentLastRecordID = 0;

                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //    int tempMaxValue = 0;
                //    int.TryParse(dt.Rows[i].ItemArray[17].ToString(), out tempMaxValue);
                //    if (tempMaxValue > nCurrentLastRecordID)
                //    {
                //        nCurrentLastRecordID = tempMaxValue;
                //    }

                //}
                //m_PrevLastRecordID = nCurrentLastRecordID;
            }

            foreach (var item in PacketCreator.CreatePackets(dt))
            {
                CurrentBuffer.Enqueue(item);
            }
            try
            {
                StartSendingPacketsToServer();
            }
            catch (Exception ex)
            {
                //Logger.Error(ex);
            }

            //Logger.Info("Exit");
        }

        private void ExtractInformationAttachementInformation(int countToExract)
        {
            int bufferSize = CurrentBuffer.Count();
            for (int i = bufferSize; i > 0; i--)
            {
                SessionHandler oSessionHndlr = CurrentBuffer.Dequeue();

                try
                {
                    LuaScriptHandler lfun = new LuaScriptHandler();
                    lfun.Execute(oSessionHndlr);
                }
                catch (Exception ex)
                {
                    //Logger.Error(ex);
                }
            }
        }

        private void StartSendingPacketsToServer()
        {
            //Logger.Info("Enter");

            byte[] output = JSONCircularBuffer.ExportToHAR(CurrentBuffer);
            if (TestTCPClient.TestConfig.TestCheck)
            {
                if (TestTCPClient.SendLazyPacketsToServer("1", output))
                {
                    foreach (SessionHandler oSessionHandler in CurrentBuffer)
                    {
                        AnalyzerManager.ProxydbObj.UpdateDB("update response set lazy_status = 3 where request_id = '" + oSessionHandler.RequestID + "'");
                    }
                }
            }
            else
            {
                if (TCPClients.SendLazyPacketsToServer("1", output))
                {
                    foreach (SessionHandler oSessionHandler in CurrentBuffer)
                    {
                        AnalyzerManager.ProxydbObj.UpdateDB("update response set lazy_status = 3 where request_id = '" + oSessionHandler.RequestID + "'");
                    }
                }
            }

            //Logger.Info("Exit");
        }
    }
}
