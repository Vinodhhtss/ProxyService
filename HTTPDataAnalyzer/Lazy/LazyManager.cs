//using log4net;
using System;
using System.Threading;

namespace HTTPDataAnalyzer.Lazy
{
    class LazyManager
    {
        private static Thread m_LazyAnalyzerThread;
        private static Thread m_LazySendThread;
        public static void Starter()
        {
            m_LazyAnalyzerThread = new Thread(new ThreadStart(LazyAnalyzer));
            m_LazyAnalyzerThread.IsBackground = true;
            m_LazyAnalyzerThread.Start();
            m_LazySendThread = new Thread(new ThreadStart(LazyMessageSend));
            m_LazySendThread.IsBackground = true;
            m_LazySendThread.Start();
        }

        static LazyManager()
        {
            try
            {
                //ILog tempLoggers = CLogger.CreateLoggers("AnalyzerLogs", "LazyMessageSendLogger");

                //if (tempLoggers != null)
                //{
                //    LazyAnalyserSender.Logger = tempLoggers;
                //}
            }
            catch (Exception)
            {
            }
        }

        public static void LazyAnalyzer()
        {
            //LazyAnalyser.Logger.Info("Enter");

            while (true)
            {
                //  while (!ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(StartLazyAnalyzer), null)) ;
                StartLazyAnalyzer(10);
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }

        }

        public static void LazyMessageSend()
        {
            //LazyAnalyserSender.Logger.Info("Enter");

            while (true)
            {
                // while (!ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(StartLazySend), null)) ;
                StartLazySend(null);
                Thread.Sleep(TimeSpan.FromMilliseconds(ConfigHandler.Config.Policies.LazyUpload));
            }

        }

        public static void StartLazyAnalyzer(object obj)
        {
            //LazyAnalyser.Logger.Info("Enter");

            if (AnalyzerManager.Analyzer == null)
            {
                AnalyzerManager.ProxydbObj = new ProxyDbs.ProxyDb(false);
            }
            LazyAnalyser db = new LazyAnalyser();
            db.Analyse();

            //LazyAnalyser.Logger.Info("Exit");
        }

        public static void StartLazySend(object obj)
        {
            //LazyAnalyserSender.Logger.Info("Enter");

            if (AnalyzerManager.ProxydbObj == null)
            {
                AnalyzerManager.ProxydbObj = new ProxyDbs.ProxyDb(false);
            }
            LazyAnalyserSender db = new LazyAnalyserSender();
            db.Analyse();

            //LazyAnalyserSender.Logger.Info("Exit");
        }

        public static void Stop()
        {
            if (m_LazyAnalyzerThread != null)
            {
                m_LazyAnalyzerThread.Abort();
            }
            if (m_LazySendThread != null)
            {
                m_LazySendThread.Abort();
            }
            if (m_LazyAnalyzerThread != null)
            {
                m_LazyAnalyzerThread.Join();
            }
            if (m_LazySendThread != null)
            {
                m_LazySendThread.Join();
            }
        }
    }
}
