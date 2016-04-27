//using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace HTTPDataAnalyzer.Lazy
{
    class LazyAnalyser
    {
      //  private static int m_PrevLastRecordID = 0;
        private static object m_CountLock = new object();

        //public static ILog Logger;

        public Queue<SessionHandler> CurrentBuffer = new Queue<SessionHandler>();

        static LazyAnalyser()
        {
            //ILog tempLogger = CLogger.CreateLoggers("AnalyzerLogs", "LazyAnalyzerLogger");
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
                //Logger.Error(ex);
            }

            //Logger.Info("Exit");
        }

        private void ReadDataFromDB()
        {
            //Logger.Info("Enter");

            DataTable dt = null;
            lock (m_CountLock)
            {
                string tempQueryString = "select * from Request INNER JOIN Response ON Response.request_id = Request.dbid where  Response.lazy_status = 0";
                dt = AnalyzerManager.ProxydbObj.GetTableFromDB(tempQueryString, "PacketDetails");

                if (dt == null || dt.Rows.Count <= 0)
                {
                    return;
                }

                //int nCurrentLastRecordID = 0;

                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //    int tempMaxValue = 0;
                //    int.TryParse(dt.Rows[i].ItemArray[15].ToString(), out tempMaxValue);
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
                StartAnalyse();
            }
            catch (Exception ex)
            {
                //Logger.Error(ex);
            }
            if (dt != null)
            {
                dt.Dispose();
            }
            //Logger.Info("Exit");
        }

        private void ExtractInformationAttachementInformation(int countToExract)
        {
            for (int i = 0; i < countToExract; i++)
            {
                SessionHandler oSessionHndlr = CurrentBuffer.ElementAt(i);

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

        private void StartAnalyse()
        {
            //Logger.Info("Enter");

            int bufferSize = CurrentBuffer.Count();
            for (int i = bufferSize; i > 0; i--)
            {
                SessionHandler oSessionHndlr = CurrentBuffer.Dequeue();
                ConditionChecker.CheckAllConditions(oSessionHndlr);
            }

            //Logger.Info("Exit");
        }
    }
}
