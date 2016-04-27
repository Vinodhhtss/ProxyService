using HTTPDataAnalyzer.Poll;
using HTTPDataAnalyzer.StoreAndForward;
using Ionic.Zip;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HTTPDataAnalyzer
{
    public class TCPClients
    {
        private static string m_ipAddress;
        private static string m_domainName;

        public static string ListeningIPInterface
        {
            get
            {

                if (IsIPv4(ConfigHandler.Config.ServerDetails.MainServerIP))
                {
                    m_ipAddress = ConfigHandler.Config.ServerDetails.MainServerIP;//"192.168.1.203";
                    m_domainName = m_ipAddress;// Util.FindDomainFromIp(m_ipAddress);
                }
                else
                {
                    try
                    {
                        m_ipAddress = Util.GetValidIPAddress(ConfigHandler.Config.ServerDetails.MainServerIP);
                        m_domainName = ConfigHandler.Config.ServerDetails.MainServerIP;//m_ipAddress;
                    }
                    catch (Exception ex)
                    {
                        //AnalyzerManager.Logger.Error(ex);
                    }

                }
                //   m_domainName = "54.151.59.162";
                return m_ipAddress;
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

        public static bool GetCertificate(string taskCode)
        {
            //Registration.ClientRegistrar.Logger.Info("Enter");

            StateObject stateObj = null;
            try
            {
                stateObj = new StateObject();
                TCPSocket.ConnectIgnoreCertError(stateObj);

                stateObj.ClientStream.AuthenticateAsClient(m_domainName);


               // byte[] bodyBytes = Encoding.ASCII.GetBytes("");
                byte[] headerBytes = BuildHeaders(taskCode, 0);

                TcpUtil.WriteHeaderData(stateObj.ClientStream, headerBytes);
                //TcpUtil.WriteData(stateObj.ClientStream, bodyBytes);

                byte[] bytesOfCert = TcpUtil.ReadDataAsBytes(stateObj.ClientStream);
                Registration.CertHandler.InstallConsoleServerCertificate(bytesOfCert);
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
                return false;
            }
            finally
            {
                stateObj.Close();
            }

            //Registration.ClientRegistrar.Logger.Info("Exit");
            return true;
        }

        public static bool UpdateClientInfo(string taskCode, byte[] output)
        {
            //SystemInfoUpdater.Logger.Info("Enter");

            StateObject stateObj = null;
            try
            {
                stateObj = new StateObject();
                TCPSocket.Connect(stateObj);

                stateObj.ClientStream.AuthenticateAsClient(m_domainName);

                byte[] headerBytes = BuildHeaders(taskCode, output.Length);
                TcpUtil.WriteHeaderData(stateObj.ClientStream, headerBytes);

                TcpUtil.WriteData(stateObj.ClientStream, output);

                string outputMsg = TcpUtil.ReadData(stateObj.ClientStream);
                if (outputMsg == string.Empty)
                {
                    return false;
                }
                else
                {
                    if (outputMsg == "Invalid ID")
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                //SystemInfoUpdater.Logger.Error(ex);
                return false;
            }
            finally
            {
                stateObj.Close();
            }

            //SystemInfoUpdater.Logger.Info("Exit");
            return true;
        }

        public static bool RegisterClientWithServer(string taskCode, byte[] output)
        {
            //Registration.ClientRegistrar.Logger.Info("Enter");

            StateObject stateObj = null;
            try
            {
                stateObj = new StateObject();
                TCPSocket.Connect(stateObj);

                stateObj.ClientStream.AuthenticateAsClient(m_domainName);

                byte[] headerBytes = BuildHeaders(taskCode, output.Length);

                TcpUtil.WriteHeaderData(stateObj.ClientStream, headerBytes);

                TcpUtil.WriteData(stateObj.ClientStream, output);

                string outputMsg = TcpUtil.ReadData(stateObj.ClientStream);
                if (outputMsg == string.Empty)
                {
                    return false;
                }
                else
                {
                    if (outputMsg == "InvalidID")
                    {
                        return false;
                    }
                }
                ConfigHandler.HostInfoes = new HostInfo();
                ConfigHandler.HostInfoes.HostID = SplitHeader(outputMsg);
                HTTPDataAnalyzer.ConfigHandler.SaveConfigFile(string.Empty);
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
                return false;
            }
            finally
            {
                stateObj.Close();
            }

            //Registration.ClientRegistrar.Logger.Info("Exit");
            return true;
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

        public static bool SendLazyPacketsToServer(string taskCode, byte[] output, bool isFromLazy = true)
        {
            //Lazy.//LazyAnalyserSender.Logger.Info("Enter");

            StateObject stateObj = null;
            try
            {
                stateObj = new StateObject();
                TCPSocket.Connect(stateObj);

                stateObj.ClientStream.AuthenticateAsClient(m_domainName);

                if (output.Length > 0)
                {
                    byte[] headerBytes = BuildHeaders(taskCode, output.Length);

                    TcpUtil.WriteHeaderData(stateObj.ClientStream, headerBytes);
                    TcpUtil.WriteData(stateObj.ClientStream, output);
                }
            }
            catch (Exception ex)
            {
                //Lazy.//LazyAnalyserSender.Logger.Info(ex);
                if (isFromLazy && output != null)
                {
                    FailHandler.LazyFailHandler.InsertInLazyFailed(output);
                }
                return false;
            }
            finally
            {
                stateObj.Close();
            }

            //Lazy.//LazyAnalyserSender.Logger.Info("Exit");
            return true;
        }

        public static bool SendAlertMessageToServer(string taskCode, byte[] output, bool isFromAlert = true)
        {
            //StoredAndForward.Logger.Info("Enter");

            StateObject stateObj = null;
            try
            {
                stateObj = new StateObject();

                TCPSocket.Connect(stateObj);
                stateObj.ClientStream.AuthenticateAsClient(m_domainName);

                if (output.Length > 0)
                {
                    byte[] headerBytes = BuildHeaders(taskCode, output.Length);
                    TcpUtil.WriteHeaderData(stateObj.ClientStream, headerBytes);
                    TcpUtil.WriteData(stateObj.ClientStream, output);
                }
            }
            catch (Exception ex)
            {
                //StoredAndForward.Logger.Error(ex);
                if (isFromAlert && output != null)
                {
                    FailHandler.AlertFailHandler.InsertInAlertFailed(output);
                }
                return false;
            }
            finally
            {
                stateObj.Close();
            }

            //StoredAndForward.Logger.Info("Exit");
            return true;
        }

        public static void SerachJobInServer(string taskCode, byte[] output)
        {
            //JobsSearcher.Logger.Info("Enter");

            StateObject stateObj = null;
            try
            {
                stateObj = new StateObject();
                TCPSocket.Connect(stateObj);

                stateObj.ClientStream.AuthenticateAsClient(m_domainName);
                byte[] headerBytes = BuildHeaders(taskCode, output.Length);

                TcpUtil.WriteHeaderData(stateObj.ClientStream, headerBytes);
                TcpUtil.WriteData(stateObj.ClientStream, output);

                bool keepAlive = true;
                while (keepAlive)
                {
                    string outputMsg = TcpUtil.ReadDataJob(stateObj.ClientStream, out keepAlive);
                    if (!string.IsNullOrEmpty(outputMsg))
                    {
                        string[] headersAndMsg = SplitConditionAndMessage(outputMsg);
                        if (headersAndMsg[0].StartsWith("AuditTypeId"))
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
            catch (Exception ex)
            {
                //JobsSearcher.Logger.Error(ex);
            }
            finally
            {
                stateObj.Close();
            }
            AuditProcessor.ExistingAudit();
            //JobsSearcher.Logger.Info("Exit");
        }

        public static void SendAuditResults(string taskCode, byte[] output, int auditJobId)
        {
            StateObject stateObj = null;
            try
            {
                stateObj = new StateObject();
                TCPSocket.Connect(stateObj);

                stateObj.ClientStream.AuthenticateAsClient(m_domainName);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(String.Format("AuditJobid: {0}", auditJobId));
                sb.AppendLine(String.Format("AuditJobType: {0}", 1));

                byte[] headerBytes = BuildHeaders(taskCode, output.Length, sb.ToString());

                TcpUtil.WriteHeaderData(stateObj.ClientStream, headerBytes);
                TcpUtil.WriteData(stateObj.ClientStream, output);
            }
            catch (Exception)
            {
            }
            finally
            {
                stateObj.Close();
            }
        }

        public static void SendFileFetchResults(string taskCode, winaudits.FileFetch ofileFetch)
        {

            StateObject stateObj = null;
            string tempPath = ofileFetch.FilePath.Replace("\"", string.Empty);
            int tryCount = 0;
            if (!File.Exists(tempPath))
            {
                tryCount = 5;
            }
            try
            {

                StringBuilder sb = new StringBuilder();
                sb.AppendLine(String.Format("AuditJobid: {0}", ofileFetch.AuditJobID));
                sb.AppendLine(String.Format("AuditJobType: {0}", 2));

                stateObj = new StateObject();
                TCPSocket.Connect(stateObj);

                stateObj.ClientStream.AuthenticateAsClient(m_domainName);

                while (tryCount < 5)
                {
                    try
                    {
                        using (FileStream stream = new FileStream(tempPath, FileMode.Open, FileAccess.Read))
                        {
                            byte[] buffer = new byte[8192];
                            int bytesRead;

                            sb.AppendLine(String.Format("FileExtension: {0}", Path.GetExtension(tempPath)));
                            byte[] headerBytes = BuildHeaders(taskCode, (long)stream.Length, sb.ToString());
                            TcpUtil.WriteHeaderData(stateObj.ClientStream, headerBytes);

                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                stateObj.ClientStream.Write(buffer);
                            }
                        }

                        winaudits.UpdateQuery.UpdateFileFetchAuditStatus(2, ofileFetch.AuditJobID);
                        break;
                    }
                    catch (Exception ex)
                    {
                        tryCount++;
                        //JobsSearcher.Logger.Error(ex);
                    }
                }

                if (tryCount == 5)
                {
                    byte[] headerBytes = BuildHeaders(taskCode, 0, sb.ToString());
                    TcpUtil.WriteHeaderData(stateObj.ClientStream, headerBytes);
                    winaudits.UpdateQuery.UpdateFileFetchAuditStatus(3, ofileFetch.AuditJobID);
                }
                byte[] end = Encoding.ASCII.GetBytes("<EOF>");
                stateObj.ClientStream.Write(end, 0, end.Length);
            }
            catch (Exception ex)
            {
                //JobsSearcher.Logger.Error(ex);
            }
            finally
            {
                stateObj.Close();
            }
        }

        public static void SendRegistryFetchResults(string taskCode, winaudits.RegistryFetch ofileFetch)
        {

            StateObject stateObj = null;
            try
            {
                int tryCount = 0;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine(String.Format("AuditJobid: {0}", ofileFetch.AuditJobID));
                sb.AppendLine(String.Format("AuditJobType: {0}", 3));
                string tempPath = ofileFetch.RegistryPath.Replace("\"", string.Empty).Trim("\\".ToCharArray());

                stateObj = new StateObject();
                TCPSocket.Connect(stateObj);

                stateObj.ClientStream.AuthenticateAsClient(m_domainName);

                while (tryCount < 5)
                {
                    try
                    {
                        string exportPath32 = Util.Export(ofileFetch.RegistryHive, tempPath, false);
                       
                        sb.AppendLine(String.Format("FileExtension: {0}", ".zip"));
                        var zipStream = new MemoryStream();
                        var zip = new ZipOutputStream(zipStream);
                        if (exportPath32 != string.Empty && File.Exists(exportPath32))
                        {
                            zip.PutNextEntry(Path.GetFileName(exportPath32));
                            byte[] fileContent = File.ReadAllBytes(exportPath32);
                            zip.Write(fileContent, 0, fileContent.Length);
                        }
                        if (Environment.Is64BitOperatingSystem && !tempPath.Contains("Wow6432Node"))
                        {
                            string exportPath64 = Util.Export(ofileFetch.RegistryHive, tempPath, true);
                            if (exportPath64 != string.Empty && File.Exists(exportPath64))
                            {
                                zip.PutNextEntry(Path.GetFileName(exportPath64));
                                byte[] fileContent = File.ReadAllBytes(exportPath64);
                                zip.Write(fileContent, 0, fileContent.Length);
                            }
                        }

                        zip.Close();
                        byte[] buffer = zipStream.ToArray();
                        byte[] headerBytes = BuildHeaders(taskCode, (long)buffer.Length, sb.ToString());
                        TcpUtil.WriteHeaderData(stateObj.ClientStream, headerBytes);
                        if (buffer.Length > 0)
                        {
                            stateObj.ClientStream.Write(buffer);
                            winaudits.UpdateQuery.UpdateRegistryFetchAuditStatus(2, ofileFetch.AuditJobID);
                        }
                        else
                        {
                            winaudits.UpdateQuery.UpdateRegistryFetchAuditStatus(3, ofileFetch.AuditJobID);
                            tryCount = 5;
                        }
                        break;
                    }
                    catch (Exception ex)
                    {
                        tryCount++;
                        //JobsSearcher.Logger.Error(ex);
                    }

                }

                if (tryCount == 5)
                {
                    byte[] headerBytes = BuildHeaders(taskCode, 0, sb.ToString());
                    TcpUtil.WriteHeaderData(stateObj.ClientStream, headerBytes);
                    winaudits.UpdateQuery.UpdateRegistryFetchAuditStatus(3, ofileFetch.AuditJobID);
                }
                byte[] end = Encoding.ASCII.GetBytes("<EOF>");
                stateObj.ClientStream.Write(end, 0, end.Length);
            }
            catch (Exception ex)
            {
                //JobsSearcher.Logger.Error(ex);
            }
            finally
            {
                stateObj.Close();
            }
        }

        public static void SearchConfigChangeInServer(string taskCode, byte[] output)
        {
            //ConfigurationDetector.Logger.Info("Enter");

            StateObject stateObj = null;
            try
            {
                stateObj = new StateObject();
                TCPSocket.Connect(stateObj);

                stateObj.ClientStream.AuthenticateAsClient(m_domainName);

                byte[] headerBytes = BuildHeaders(taskCode, output.Length);

                TcpUtil.WriteHeaderData(stateObj.ClientStream, headerBytes);
               // TcpUtil.WriteData(stateObj.ClientStream, output);

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

            //ConfigurationDetector.Logger.Info("Exit");
        }

        private static byte[] BuildHeaders(string taskCode, long contentLength, string auditParams = "")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("HTTP/1.1 200 Connection established");
            sb.AppendLine(String.Format("Timestamp: {0}", DateTime.UtcNow.ToString()));
            sb.AppendLine(String.Format("TaskCode: {0}", taskCode));
            if (taskCode != "0" && taskCode != "00")
            {
                sb.AppendLine(String.Format("HostGuid: {0}", ConfigHandler.Config.HostInfoes.HostID));
            }

            if (taskCode == "0")
            {
                sb.AppendLine(String.Format("PolicyID: {0}", ConfigHandler.Config.Policies.PolicyId));
            }

            if (taskCode == "00")
            {
                sb.AppendLine(String.Format("ServerName: {0}", ConfigHandler.Config.ServerDetails.MainServerIP));
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
    }
}
