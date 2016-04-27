using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace HTTPProxyServer
{
    public class SecurePipeClient
    {
        String m_serverpipe;
        static object m_CountLock = new object();

        public SecurePipeClient(string serverpipe)
        {
            m_serverpipe = serverpipe;
        }

        public bool SendMessage(string message, out string response)
        {
            response = string.Empty;
            PipeStream pipe = null;

            PipeClient cli = new PipeClient(".", m_serverpipe);
            int tryCount = 0;
            while (pipe == null)
            {
                try
                {
                    pipe = cli.Connect(10);
                    tryCount++;
                }
                catch (Exception ex)
                {
                    ////TCPClientProcessor.Proxylog.Logger.Error(ex);
                    return false;
                }

                if (tryCount > 1)
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Server Connection attempt " + tryCount.ToString());
                }
            }

            if (pipe != null && pipe.IsConnected)
            {

                Byte[] sendmsg = Encoding.UTF8.GetBytes(message);
                pipe.Write(sendmsg, 0, sendmsg.Length);
                response = message + "Ended";

                pipe.Close();
            }

            return true;
        }
    }
}
