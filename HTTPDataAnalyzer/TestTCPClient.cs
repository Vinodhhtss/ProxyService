using HTTPDataAnalyzer.Poll;
using HTTPDataAnalyzer.Registration;
using HTTPDataAnalyzer.StoreAndForward;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HTTPDataAnalyzer
{
    public class TestTCPClient
    {
        private static string m_ipAddress;
        private static string m_domainName;
        public static TestConfigure TestConfig = null;
        public static string GuidFilePath = System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                           "ProxyService\\TestConfig"), "HostIds.csv");
        public static ConcurrentDictionary<string, string> UpdateClient = new ConcurrentDictionary<string, string>();

        public static bool IsRegistrationSuccess = true;
        static TestTCPClient()
        {
            try
            {
                TestConfigure.Serialize();
                string test = ListeningIPInterface;
                int int1 = ListeningPort;
                TestConfig = TestConfigure.GetConfig();

                //if (ConstantVariables.IsInDebug && TestConfig.TestCheck && File.Exists(GuidFilePath))
                //{
                //    ReadHostIds();
                //}
            }
            catch (Exception)
            {
                TestConfig = new TestConfigure();
                TestConfig.TestCheck = false;
            }
            TestConfig = new TestConfigure();
            TestConfig.TestCheck = false;
        }

        public static void ReadHostIds()
        {
            if (!TestConfig.TestCheck || !File.Exists(GuidFilePath))
            {
                return;
            }
            UpdateClient.Clear();
            using (var sr = new StreamReader(GuidFilePath))
            {
                while (sr.Peek() >= 0)
                {
                    string tempLine = sr.ReadLine();
                    string[] idAndName = tempLine.Split(new string[] { ": " }, StringSplitOptions.None);
                    if (idAndName.Count() > 1)
                        UpdateClient.TryAdd(idAndName[0], idAndName[1]);
                }
            }
        }

        public static string ListeningIPInterface
        {
            get
            {
                if (IsIPv4(ConfigHandler.Config.ServerDetails.MainServerIP))
                {
                    m_ipAddress = ConfigHandler.Config.ServerDetails.MainServerIP;
                    m_domainName = m_ipAddress;// Util.FindDomainFromIp(m_ipAddress);
                }
                else
                {
                    try
                    {
                        m_ipAddress = Util.GetValidIPAddress(ConfigHandler.Config.ServerDetails.MainServerIP);
                        m_domainName = m_ipAddress;
                    }
                    catch (Exception ex)
                    {
                        //AnalyzerManager.Logger.Error(ex);
                    }

                }
                return m_ipAddress;
            }
            set
            {
                m_ipAddress = value;
            }
        }

        public static int ListeningPort
        {
            get
            {
                return ConfigHandler.Config.ServerDetails.MainServerPort;
            }
        }

        public static bool IsIPv4(string value)
        {
            IPAddress address;

            if (IPAddress.TryParse(value, out address))
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool UpdateClientInfo(string taskCode, byte[] bodyBytes)
        {
            //ConfigurationDetector.Logger.Info("Enter");

            try
            {
                var tasks = new List<Task>();
                for (int i = 0; i < TestConfig.TestCount; i++)
                {
                    ThreadPoolCommonClass common = new ThreadPoolCommonClass(taskCode, bodyBytes, i);
                    tasks.Add(Task.Factory.StartNew(UpdateClientInfo, common));
                }
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception)
            {
                return false;
            }

            //ConfigurationDetector.Logger.Info("Exit");
            return true;
        }

        public static void UpdateClientInfo(object updateGetObj)
        {

            ThreadPoolCommonClass common = (ThreadPoolCommonClass)updateGetObj;
            SystemConfiguration sysConfig;
            string tempConfig = Encoding.ASCII.GetString(common.Output);
            sysConfig = JsonConvert.DeserializeObject<SystemConfiguration>(tempConfig);

            string tempHostName;

            UpdateClient.TryGetValue(UpdateClient.Keys.ElementAt(common.ThreadIndx), out tempHostName);
            sysConfig.HostName = tempHostName;

            byte[] configByte = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(sysConfig));
            StateObject stateObj = null;
            try
            {
                stateObj = new StateObject();
                TCPSocket.Connect(stateObj);
                stateObj.ClientStream.AuthenticateAsClient(m_domainName);

                byte[] headerBytes = BuildHeaders(common.TaskCode, configByte.Length, UpdateClient.Keys.ElementAt(common.ThreadIndx));
                TcpUtil.WriteHeaderData(stateObj.ClientStream, headerBytes);

                TcpUtil.WriteData(stateObj.ClientStream, configByte);

                string outputMsg = TcpUtil.ReadData(stateObj.ClientStream);
                if (outputMsg == string.Empty)
                {
                    return;
                }
                else
                {
                    if (outputMsg == "Invalid ID")
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                //ConfigurationDetector.Logger.Error(ex);
            }
            finally
            {
                stateObj.Close();
            }
        }

        public static bool RegisterClientWithServer(string taskCode, byte[] testOutput)
        {
            //HTTPDataAnalyzer.Registration.ClientRegistrar.Logger.Info("Enter");
            UpdateClient.Clear();
            try
            {
                var tasks = new List<Task>();
                for (int i = 0; i < TestConfig.TestCount; i++)
                {
                    ThreadPoolCommonClass common = new ThreadPoolCommonClass(taskCode, testOutput, i);
                    tasks.Add(Task.Factory.StartNew(RegisterClientWithServer, common));
                }
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                //HTTPDataAnalyzer.Registration.ClientRegistrar.Logger.Error(ex);
            }

            //HTTPDataAnalyzer.Registration.ClientRegistrar.Logger.Info("Exit");
            return IsRegistrationSuccess;
        }

        public static void RegisterClientWithServer(object obj)
        {
            ThreadPoolCommonClass common = (ThreadPoolCommonClass)obj;
            SystemConfiguration sysConfig;
            string tempConfig = Encoding.ASCII.GetString(common.Output);
            sysConfig = JsonConvert.DeserializeObject<SystemConfiguration>(tempConfig);

            sysConfig.HostName = common.ThreadIndx.ToString() + sysConfig.HostName + DateTime.Now.ToString(ConstantVariables.DATE_FORMAT);

            byte[] configByte = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(sysConfig));
            StateObject stateObj = null;
            try
            {
                stateObj = new StateObject();
                TCPSocket.Connect(stateObj);

                stateObj.ClientStream.AuthenticateAsClient(m_domainName);
                byte[] headerBytes = BuildHeaders(common.TaskCode, configByte.Length, string.Empty);

                TcpUtil.WriteHeaderData(stateObj.ClientStream, headerBytes);
                TcpUtil.WriteData(stateObj.ClientStream, configByte);

                string outputMsg = TcpUtil.ReadData(stateObj.ClientStream);
                if (outputMsg == string.Empty)
                {
                    IsRegistrationSuccess = false;
                    return;
                }
                else
                {
                    if (outputMsg == "InvalidID")
                    {
                        IsRegistrationSuccess = false;
                        return;
                    }
                }
                if (common.ThreadIndx == 0)
                {
                    ConfigHandler.HostInfoes = new HostInfo();
                    ConfigHandler.HostInfoes.HostID = SplitHeader(outputMsg);
                    HTTPDataAnalyzer.ConfigHandler.SaveConfigFile(string.Empty);
                }
                UpdateClient.TryAdd(SplitHeader(outputMsg), sysConfig.HostName);
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
                IsRegistrationSuccess = false;
            }
            finally
            {
                stateObj.Close();
            }
            IsRegistrationSuccess = true;
        }
        public static bool SendLazyPacketsToServer(string taskCode, byte[] output, bool isFromAlert = true)
        {
            //Lazy.//LazyAnalyserSender.Logger.Info("Enter");

            try
            {
                var tasks = new List<Task>();
                for (int i = 0; i < TestConfig.TestCount; i++)
                {
                    ThreadPoolCommonClass common = new ThreadPoolCommonClass(taskCode, output, i, isFromAlert);
                    tasks.Add(Task.Factory.StartNew(SendLazyPacketsToServer, common));
                }
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                //StoredAndForward.Logger.Error(ex);
            }

            //Lazy.//LazyAnalyserSender.Logger.Info("Exit");
            return true;
        }

        public static void SendLazyPacketsToServer(object obj)
        {
            ThreadPoolCommonClass common = (ThreadPoolCommonClass)obj;
            StateObject stateObj = null;
            try
            {
                stateObj = new StateObject();
                TCPSocket.Connect(stateObj);

                stateObj.ClientStream.AuthenticateAsClient(m_domainName);

                if (common.Output.Length > 0)
                {
                    byte[] headerBytes = BuildHeaders(common.TaskCode, common.Output.Length, UpdateClient.Keys.ElementAt(common.ThreadIndx));

                    TcpUtil.WriteHeaderData(stateObj.ClientStream, headerBytes);
                    TcpUtil.WriteData(stateObj.ClientStream, common.Output);
                }
            }
            catch (Exception ex)
            {
                if (common.IsFromLazy && common.Output != null)
                {
                    FailHandler.LazyFailHandler.InsertInLazyFailed(common.Output);
                }
                //Lazy.//LazyAnalyserSender.Logger.Error(ex);
              
            }
            finally
            {
                stateObj.Close();
            }
        }

        public static bool SendAlertMessageToServer(string taskCode, byte[] output, bool isFromAlert = true)
        {
            //StoredAndForward.Logger.Info("Enter");

            try
            {
                var tasks = new List<Task>();
                for (int i = 0; i < TestConfig.TestCount; i++)
                {
                    ThreadPoolCommonClass common = new ThreadPoolCommonClass(taskCode, output, i, isFromAlert);
                    tasks.Add(Task.Factory.StartNew(SendAlertMessageToServer, common));
                }
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                //StoredAndForward.Logger.Info(ex);
            }

            //StoredAndForward.Logger.Info("Exit");
            return true;
        }

        public static void SendAlertMessageToServer(object obj)
        {
            ThreadPoolCommonClass common = (ThreadPoolCommonClass)obj;

            StateObject stateObj = null;
            try
            {
                stateObj = new StateObject();

                TCPSocket.Connect(stateObj);
                stateObj.ClientStream.AuthenticateAsClient(m_domainName);

                if (common.Output.Length > 0)
                {
                    byte[] headerBytes = BuildHeaders(common.TaskCode, common.Output.Length, UpdateClient.Keys.ElementAt(common.ThreadIndx));
                    TcpUtil.WriteHeaderData(stateObj.ClientStream, headerBytes);
                    TcpUtil.WriteData(stateObj.ClientStream, common.Output);
                }
            }
            catch (Exception)
            {
                if (common.IsFromAlert && common.Output != null)
                {
                    FailHandler.AlertFailHandler.InsertInAlertFailed(common.Output);
                }
            }
            finally
            {
                stateObj.Close();
            }
        }

        public static void SerachJobInServer(string taskCode, byte[] output)
        {
            //JobsSearcher.Logger.Info("Enter");

            try
            {
                var tasks = new List<Task>();
                for (int i = 0; i < TestConfig.TestCount; i++)
                {
                    ThreadPoolCommonClass common = new ThreadPoolCommonClass(taskCode, output, i);
                    tasks.Add(Task.Factory.StartNew(SerachJobInServer, common));
                }
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                //JobsSearcher.Logger.Info(ex);
            }

            AuditProcessor.ExistingAudit();

            //JobsSearcher.Logger.Info("Exit");
        }

        public static void SerachJobInServer(object obj)
        {
            ThreadPoolCommonClass common = (ThreadPoolCommonClass)obj;

            StateObject stateObj = null;
            try
            {
                stateObj = new StateObject();
                TCPSocket.Connect(stateObj);

                stateObj.ClientStream.AuthenticateAsClient(m_domainName);

                byte[] headerBytes = BuildHeaders(common.TaskCode, common.Output.Length, UpdateClient.Keys.ElementAt(common.ThreadIndx));

                TcpUtil.WriteHeaderData(stateObj.ClientStream, headerBytes);
                TcpUtil.WriteData(stateObj.ClientStream, common.Output);

                bool keepAlive = true;
                while (keepAlive)
                {
                    string outputMsg = TcpUtil.ReadDataJob(stateObj.ClientStream, out keepAlive);
                    if (!string.IsNullOrEmpty(outputMsg))
                    {
                        string[] headersAndMsg = SplitConditionAndMessage(outputMsg);
                        if (headersAndMsg[0] == "WinAudit")
                        {
                            AuditProcessor.ProcessAudit(headersAndMsg);
                            TcpUtil.WriteData(stateObj.ClientStream, Encoding.ASCII.GetBytes("received"));
                        }
                        else
                        {
                            DataTable dt = AnalyzerManager.ProxydbObj.GetTableFromDB(headersAndMsg[0], "PacketDetails");
                            if (dt != null && dt.Rows.Count > 0)
                            {
                                string tempOut = JsonConvert.SerializeObject(dt);
                                TcpUtil.WriteData(stateObj.ClientStream, Encoding.ASCII.GetBytes(tempOut));
                            }
                            else
                            {
                                TcpUtil.WriteData(stateObj.ClientStream, Encoding.ASCII.GetBytes(string.Empty));
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                stateObj.Close();
            }
        }

        public static void SendAuditResults(string taskCode, byte[] output, int auditJobId)
        {
            //JobsSearcher.Logger.Info("Enter");

            try
            {
                var tasks = new List<Task>();
                for (int i = 0; i < TestConfig.TestCount; i++)
                {
                    ThreadPoolCommonClass common = new ThreadPoolCommonClass(taskCode, output, i, auditJobId);
                    tasks.Add(Task.Factory.StartNew(SendAuditResults, common));
                }
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                //JobsSearcher.Logger.Info(ex);
            }

            //JobsSearcher.Logger.Info("Exit");
        }

        public static void SendAuditResults(object obj)
        {
            ThreadPoolCommonClass common = (ThreadPoolCommonClass)obj;
            StateObject stateObj = null;
            try
            {
                stateObj = new StateObject();
                TCPSocket.Connect(stateObj);

                stateObj.ClientStream.AuthenticateAsClient(m_domainName);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(String.Format("AuditJobid: {0}", common.AuditJobId));
                sb.AppendLine(String.Format("AuditJobType: {0}", string.Empty));
                byte[] headerBytes = BuildHeaders(common.TaskCode, common.Output.Length, UpdateClient.Keys.ElementAt(common.ThreadIndx), sb.ToString());

                TcpUtil.WriteHeaderData(stateObj.ClientStream, headerBytes);
                TcpUtil.WriteData(stateObj.ClientStream, common.Output);
            }
            catch (Exception)
            {
            }
            finally
            {
                stateObj.Close();
            }
        }

        public static void SearchConfigChangeInServer(string taskCode, byte[] output)
        {
            //ConfigurationDetector.Logger.Info("Enter");

            try
            {
                var tasks = new List<Task>();
                for (int i = 0; i < TestConfig.TestCount; i++)
                {
                    ThreadPoolCommonClass configChange = new ThreadPoolCommonClass(taskCode, output, i);
                    tasks.Add(Task.Factory.StartNew(SearchConfigChangeInServer, configChange));
                }
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                //ConfigurationDetector.Logger.Info(ex);
            }

            //ConfigurationDetector.Logger.Info("Exit");
        }

        public static void SearchConfigChangeInServer(object obj)
        {
            StateObject stateObj = null;
            ThreadPoolCommonClass common = (ThreadPoolCommonClass)obj;
            try
            {
                stateObj = new StateObject();
                TCPSocket.Connect(stateObj);

                stateObj.ClientStream.AuthenticateAsClient(m_domainName);

                byte[] headerBytes = BuildHeaders(common.TaskCode, common.Output.Length, UpdateClient.Keys.ElementAt(common.ThreadIndx));

                TcpUtil.WriteHeaderData(stateObj.ClientStream, headerBytes);
                TcpUtil.WriteData(stateObj.ClientStream, common.Output);

                string outputMsg = TcpUtil.ReadData(stateObj.ClientStream);
                if (outputMsg == string.Empty)
                {
                    return;
                }

                if (outputMsg == "InvalidID")
                {
                    return;
                }
                HTTPDataAnalyzer.ConfigHandler.SaveConfigFile(outputMsg);
            }
            catch (Exception ex)
            {
                //ConfigurationDetector.Logger.Error(ex);
            }
            finally
            {
                stateObj.Close();
            }
        }

        private static byte[] BuildHeaders(string taskCode, long contentLength, string hostId, string auditParams = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("Timestamp: {0}", DateTime.Now.ToString()));
            sb.AppendLine(String.Format("TaskCode: {0}", taskCode));
            if (taskCode != "0" && taskCode != "00")
            {
                sb.AppendLine(String.Format("HostGuid: {0}", hostId));
            }

            if (taskCode == "0")
            {
                sb.AppendLine(String.Format("PolicyID: {0}", ConfigHandler.Config.Policies.PolicyId));
            }

            if (taskCode == "00")
            {
                sb.AppendLine(String.Format("ServerName: {0}", ListeningIPInterface));
            }

            if (taskCode == "4")
            {
                sb.AppendLine(String.Format("CurrentVersion: {0}", ConfigHandler.Config.Policies.PolicyVersion));
            }

            if (auditParams != "")
            {
                sb.AppendLine(auditParams);
            }
            sb.AppendLine(String.Format("ContentLength: {0}", contentLength));
            sb.AppendLine(String.Format("Connection: Close"));
            sb.AppendLine();

            byte[] headerBytes = Encoding.ASCII.GetBytes(sb.ToString());
            return headerBytes;
        }

        private static string[] SplitConditionAndMessage(string outputMsg)
        {
            return outputMsg.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string SplitHeader(string input)
        {
            string output = string.Empty;
            try
            {
                string[] headerAndValue = input.Split(new string[] { ": " }, StringSplitOptions.None);
                if (headerAndValue.Count() == 2)
                {
                    output = headerAndValue[1];
                }
                else
                {
                    output = headerAndValue[0];
                }
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
            }
            return output;
        }


        public static void GetGuids()
        {
            File.Delete(GuidFilePath);
            TextWriter GuidWrite = new StreamWriter(GuidFilePath, true);
            foreach (KeyValuePair<string, string> item in UpdateClient)
            {

                try
                {
                    GuidWrite.WriteLine(item.Key + ": " + item.Value);
                }
                catch (Exception ex)
                {
                    //HTTPDataAnalyzer.Registration.ClientRegistrar.Logger.Error(ex);
                }
            }
            GuidWrite.Close();
        }
    }

    public class ThreadPoolCommonClass
    {
        public string TaskCode;
        public byte[] Output;
        public int ThreadIndx;
        public bool IsFromLazy;
        public bool IsFromAlert;
        public int AuditJobId;


        public ThreadPoolCommonClass(string taskCode, byte[] output)
        {
            TaskCode = taskCode;
            Output = output;
        }

        public ThreadPoolCommonClass(string taskCode, byte[] output, int threadIndex)
        {
            TaskCode = taskCode;
            Output = output;
            ThreadIndx = threadIndex;
        }

        public ThreadPoolCommonClass(string taskCode, byte[] output, int threadIndex, bool isFormLazy)
        {
            TaskCode = taskCode;
            Output = output;
            ThreadIndx = threadIndex;
            IsFromLazy = isFormLazy;
        }

        public ThreadPoolCommonClass(string taskCode, byte[] output, int threadIndex, int auditJobId)
        {
            TaskCode = taskCode;
            Output = output;
            ThreadIndx = threadIndex;
            AuditJobId = auditJobId;
        }
    }
}
