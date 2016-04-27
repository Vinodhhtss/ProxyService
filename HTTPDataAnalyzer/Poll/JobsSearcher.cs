//using log4net;
using System;
using System.Text;
using System.Threading;

namespace HTTPDataAnalyzer.Poll
{
    public class JobsSearcher
    {
        public static ProxyDbs.ProxyDb DBHandleJob = new ProxyDbs.ProxyDb();
        //public static ILog Logger;
        public static void Start()
        {
            //Logger.Info("Enter");

            while (true)
            {
                //while (!ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(SearchJobs), null)) ;
                SearchJobs(null);
                Thread.Sleep(TimeSpan.FromSeconds(30));
            }
        }
        static JobsSearcher()
        {
            try
            {
                //ILog tempLogger = CLogger.CreateLoggers("AnalyzerLogs", "JobsSearcherLogger");
                //if(tempLogger!=null)
                //{
                //    Logger = tempLogger;
                //}
            }
            catch (Exception)
            {
            }
        }

        public static void SearchJobs(object obj)
        {
            //Logger.Info("Enter");

            try
            {
                byte[] output = Encoding.ASCII.GetBytes("Jobs search");

                if (TestTCPClient.TestConfig.TestCheck)
                {
                    TestTCPClient.SerachJobInServer("3", output);
                }
                else
                {
                    TCPClients.SerachJobInServer("3", output);
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
