using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace HTTPProxyServer
{
    public class AlertProcessor
    {
        public static ConcurrentQueue<SessionHandler> CurrentAlert = new ConcurrentQueue<SessionHandler>();

        public static Dictionary<string, string> Conditions = new Dictionary<string, string>();

        static AlertProcessor()
        {
            Conditions.Add("ClientNameAlert", "HTTPProxyServer.ClientNameAlert");
            Conditions.Add("MIMETypeAlert", "HTTPProxyServer.MIMETypeAlert");
            Conditions.Add("ContentTypes", "HTTPProxyServer.ContentTypes");
        }
        public static void AlertQueueProcessor()
        {
            while (true)
            {
                if (CurrentAlert.Count != 0)
                {
                    SessionHandler oSess = null;
                    CurrentAlert.TryDequeue(out oSess);
                    if (oSess != null)
                    {
                        CreatClientPipe(oSess);
                    }
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

        public static void ProcessPacket(SessionHandler oSessionHandler)
        {
            string tempAssemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            foreach (var item in Conditions)
            {
                try
                {
                    var obj = Activator.CreateInstance(tempAssemblyName, item.Value);
                    var condition = (IAlert)obj.Unwrap();
                    if (condition.IsAlert(oSessionHandler))
                    {
                        AddAlertToQueue(oSessionHandler);
                    }
                }
                catch (Exception ex)
                {
                    ////TCPClientProcessor.Proxylog.Logger.Error(ex);
                }
            }
        }

        public static void AddAlertToQueue(SessionHandler oSessionHandler)
        {
            CurrentAlert.Enqueue(oSessionHandler);
        }

        public static void CreatClientPipe(Object obj)
        {
            SessionHandler index = (SessionHandler)obj;
            SecurePipeClient sc = new SecurePipeClient(ConstantVariables.PIPE_SERVER_NAME);

            string message = index.RequestID.ToString();
            string response = string.Empty;

            sc.SendMessage(message, out response);
        }
    }
}
