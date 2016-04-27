using System;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;

namespace HTTPProxyServer
{
   public class TcpHelperUtil
    {
        public static ConcurrentDictionary<int, string> ProcessNames = new ConcurrentDictionary<int, string>();
        private static Thread ProcessNameHandler = new Thread(new ParameterizedThreadStart(ClearProcessNames));

        public static void ClearProcessNames(Object obj)
        {
            while(true)
            {
                try
                {
                    string tempOut;
                    ProcessNames.TryRemove(ProcessNames.Keys.First(), out tempOut);
                }
                catch (Exception)
                {
                }
                Thread.Sleep(TimeSpan.FromSeconds(30));
            }

        }

        public static TcpClientObject GetPortDetails(Socket client)
        {
            TcpClientObject objClient = new TcpClientObject(client, -1, string.Empty);
            try
            {
                objClient.ClientID = TcpClientIDNative.LocalPortToPIDMap(((IPEndPoint)client.RemoteEndPoint).Port);
            }
            catch (Exception ex)
            {
                ////TCPClientProcessor.Proxylog.Logger.Error(ex);
            }

            return objClient;
        }

        /// <summary>
        /// Using WMI API to get process name 
        /// Process.GetProcessById, because if calling process ins
        /// 32 bit and given process id is 64 bit, caller will not be able to
        /// get the process name
        /// </summary>
        /// <param name="processID"></param>
        /// <returns></returns>
        public static string GetProcessName(int processID, bool isModuleName)
        {
            try
            {
                using (Process p = Process.GetProcessById(processID))
                {
                    if (isModuleName)
                    {
                        return p.Modules[0].FileName;
                    }
                    else
                    {
                        return p.ProcessName;
                    }
                }
            }
            catch (Exception ex)
            {
                ////TCPClientProcessor.Proxylog.Logger.Error(ex);
            }

            return string.Empty;
        }

        public static string GetMainModuleFilepath(int processId)
        {
            if(processId == -1)
            {
                return string.Empty;
            }
            try
            {
                string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
                using (var searcher = new ManagementObjectSearcher(wmiQueryString))
                {
                    using (var results = searcher.Get())
                    {
                        ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                        if (mo != null)
                        {
                            return (string)mo["ExecutablePath"];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ////TCPClientProcessor.Proxylog.Logger.Error(ex);
            }
            return string.Empty;
        }
    }
}