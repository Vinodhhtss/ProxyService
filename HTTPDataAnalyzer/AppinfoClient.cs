using System;
using System.Threading;

namespace HTTPDataAnalyzer
{
    class AppinfoClient
    {
        //static bool m_stop = false;
        static pipecomm.SecurePipeClient sc;

        static void MessageLoop()
        {
            int index = 0;
            while (true)
            {
                Thread.Sleep(100);
                if (index < 2)
                {
                    string message = "Test Request" + index;
                    string response = string.Empty;
                    sc.SendMessage(message, out response);
                    index++;
                }
            }
        }

        public bool Start()
        {
            bool retval = true;
            try
            {
                sc = new pipecomm.SecurePipeClient();

                retval = sc.Init("T1Pipe");
                Thread m_thread = new Thread(MessageLoop);
                m_thread.Start();
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
                return false;
            }
            return retval;
        }

        public bool Stop()
        {
            try
            {

            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
                return false;
            }
            return true;
        }
    }
}
