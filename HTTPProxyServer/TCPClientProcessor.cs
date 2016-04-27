using CertificateManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace HTTPProxyServer
{
    public class TCPClientProcessor
    {
        public static string UserName = string.Empty;
        //Rajesh. Logs
        public static CLogger Reqlog;
        public static CLogger Proxylog;

        //Rajesh ProxyUI
        public static ProxyUI.WebMap m_WMap;

        static TCPClientProcessor()
        {
            /////Rajesh ProxyUI
            m_WMap = new ProxyUI.WebMap();

            //Rajesh. Add a log for requests.
            //Reqlog = new CLogger("ReqLog", "ProxyAdmin", 1, true);
            //Proxylog = new CLogger("ProxyLog", "ProxyAdmin", 1, true);
            GetWindowsUserName();
        }

        public static void ProcessTCPClient(Object obj)
        {
            TcpClientObject objClient = (TcpClientObject)obj;
            try
            {
                if (TcpHelperUtil.ProcessNames.ContainsKey(objClient.ClientID))
                {
                    objClient.ClientName = TcpHelperUtil.ProcessNames[objClient.ClientID];
                }
                else
                {
                    objClient.ClientName = TcpHelperUtil.GetMainModuleFilepath(objClient.ClientID);
                    TcpHelperUtil.ProcessNames.TryAdd(objClient.ClientID, objClient.ClientName);

                }
                DoHttpProcessing(objClient);
            }
            catch (Exception ex)
            {
                ////TCPClientProcessor.Proxylog.Logger.Error(ex);
            }
            finally
            {
                if (objClient.Client.Connected)
                {
                    objClient.Client.Dispose();
                }
            }
        }

        private static void DoHttpProcessing(TcpClientObject clientObj)
        {
            Stream clientStream = null;
            CustomBinaryReader clientStreamReader = null;
            string tunnelHostName = null;

            int tunnelPort = 0;
            try
            {
                clientStream = new BufferedStream(new NetworkStream(clientObj.Client));

                clientStreamReader = new CustomBinaryReader(clientStream, Encoding.ASCII);
                // StreamReader sr = new StreamReader(clientStream);
                string securehost = null;

                List<string> requestLines = new List<string>();
                ReadLinesInStream(clientStreamReader, requestLines);

                //read the first line HTTP command
                String httpCmd = requestLines.Count > 0 ? requestLines[0] : null;
                if (String.IsNullOrEmpty(httpCmd))
                {
                    return;
                }
                //break up the line into three components
                String[] splitBuffer = httpCmd.Split(ConstantVariables.SPACE_Split, 3);

                string method = splitBuffer[0];
                string remoteUri = splitBuffer[1];
                //CLogger oclogger = new CLogger(remoteUri.ToString(), UserName, clientObj.ThreadIndex, false);

                string RequestVersion = "HTTP/1.1";

                //////////// Rajesh ProxyUI////////////////////////////////////
                ////// We just receieved the packet and method /////////

                if (splitBuffer[1].Contains(ConstantVariables.PROXY_UI_URL) == true)
                {
                    ProxyUI.ProxyResponder prResponder = new ProxyUI.ProxyResponder();
                    prResponder.ClientStream = clientStream;
                    prResponder.wbmap = m_WMap;
                    prResponder.PopulateRequestHeaders(ref requestLines);
                    prResponder.RequestURL = remoteUri.ToString();
                    prResponder.SplitAndSetUrl(prResponder.RequestURL);
                    prResponder.httpVersion = RequestVersion;
                    prResponder.Method = method;
                    prResponder.RespondToRequest();
                    return;
                }

                ///////////////////////////////////////////////////////

                if (splitBuffer[0].ToUpper() == "CONNECT")
                {
                    //Browser wants to create a secure tunnel
                    //instead = we are going to perform a man in the middle "attack"
                    //the user's browser should warn them of the certification errors, so we need to install our root certficate in users machine as Certificate Authority.
                    remoteUri = "https://" + splitBuffer[1];

                    ////Rajesh.  Write remote URI to log.
                    //Reqlog.Logger.Info(ConstantVariables.REQ + remoteUri);

                    //oclogger.Logger.Info("Current url is: " + splitBuffer[1]);
                    tunnelHostName = splitBuffer[1].Split(ConstantVariables.COLON)[0];
                    int.TryParse(splitBuffer[1].Split(ConstantVariables.COLON)[1], out tunnelPort);
                    if (tunnelPort == 0) tunnelPort = 80;

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(RequestVersion + " 200 Connection established");
                    sb.AppendLine(String.Format("Timestamp: {0}", DateTime.Now.ToString()));
                    sb.AppendLine(String.Format("Connection: Close"));
                    sb.AppendLine();
                    byte[] byteToWriteinStream = Encoding.UTF8.GetBytes(sb.ToString());
                    clientStream.Write(byteToWriteinStream, 0, byteToWriteinStream.Length);
                    clientStream.Flush();
                    if (tunnelPort != 443)
                    {
                        TCPTunneling.sendRaw(tunnelHostName, tunnelPort, clientStreamReader.BaseStream);
                        return;
                    }

                    SslStream sslStream = new SslStream(clientStream, false);

                    String strURL = splitBuffer[1].Substring(0, splitBuffer[1].IndexOf(ConstantVariables.COLON));

                    try
                    {
                        X509Certificate2 certificate = CertMaker.FindCert(strURL);
                        sslStream.AuthenticateAsServer(certificate, false, SslProtocols.Tls | SslProtocols.Ssl2 | SslProtocols.Ssl3, false);
                    }
                    catch (Exception ex)
                    {
                        //oclogger.Logger.Error("Current URL " + strURL, ex);
                        if (sslStream != null)
                        {
                            sslStream.Dispose();
                        }

                        return;
                    }
                    clientStreamReader = new CustomBinaryReader(sslStream, Encoding.UTF8);

                    //HTTPS server created - we can now decrypt the client's traffic
                    clientStream = sslStream;
                    ReadLinesInStream(clientStreamReader, requestLines);

                    //read the new http command.
                    httpCmd = requestLines.Count > 0 ? requestLines[0] : null;
                    if (String.IsNullOrEmpty(httpCmd))
                    {
                        return;
                    }
                    securehost = remoteUri;
                }
                else
                {
                    //oclogger.Logger.Info("Current url is " + splitBuffer[1]);
                    //Reqlog.Logger.Info(ConstantVariables.REQ + splitBuffer[1]);
                }

                SessionHandler oSessionHndlr = null;
                CLogger luaLogger = null;
                while (!String.IsNullOrEmpty(httpCmd))
                {
                    if (oSessionHndlr != null)
                    {
                        luaLogger = oSessionHndlr.LuaLogger;
                    }
                    oSessionHndlr = new SessionHandler(clientObj.ThreadIndex);

                    try
                    {
                        //Rajesh. Start time
                        oSessionHndlr.StartTime = DateTime.UtcNow.ToBinary().ToString();
                        oSessionHndlr.RequestStarted = DateTime.UtcNow;
                        splitBuffer = httpCmd.Split(ConstantVariables.SPACE_Split, 3);

                        if (splitBuffer.Length != 3)
                        {
                            TCPTunneling.sendRaw(httpCmd, tunnelHostName, ref requestLines, oSessionHndlr.IsSecure, clientStreamReader.BaseStream);
                            return;
                        }
                        method = splitBuffer[0].Trim();
                        remoteUri = splitBuffer[1].Trim();

                        if (securehost != null)
                        {
                            remoteUri = securehost + splitBuffer[1].Trim();
                            oSessionHndlr.IsSecure = true;
                        }
                        //oclogger.Logger.Info("Current url is " + remoteUri);
                        //Reqlog.Logger.Info(ConstantVariables.REQ + remoteUri);

                        //construct the web request that we are going to issue on behalf of the client.

                        oSessionHndlr.StopWatch.Start();

                        oSessionHndlr.FirstHeaderLine = requestLines[0];
                        oSessionHndlr.ProxyRequest = (HttpWebRequest)HttpWebRequest.Create(remoteUri.ToString().Trim());
                        oSessionHndlr.RequestURL = remoteUri.ToString();
                        oSessionHndlr.ProxyRequest.Proxy = ProxyConfig.UpStreamWebProxy;
                        oSessionHndlr.ProxyRequest.UseDefaultCredentials = true;
                        oSessionHndlr.ProxyRequest.Method = method;
                        oSessionHndlr.Method = method;
                        oSessionHndlr.ProxyRequest.ProtocolVersion = new Version(1, 1);
                        oSessionHndlr.ProxyRequest.AutomaticDecompression = DecompressionMethods.None;
                        oSessionHndlr.ClientStream = clientStream;
                        oSessionHndlr.ClientStreamReader = clientStreamReader;
                        oSessionHndlr.ProxyRequest.KeepAlive = false;
                        oSessionHndlr.ProxyRequest.AllowWriteStreamBuffering = false;

                        try
                        {
                            for (int i = 1; i < requestLines.Count; i++)
                            {
                                var rawHeader = requestLines[i];
                                String[] header = rawHeader.Trim().Split(ConstantVariables.COLON_SPACE_SPLIT, 2, StringSplitOptions.None);
                                if (!String.IsNullOrEmpty(header[0].Trim()))
                                {
                                    if (header.Length > 1)
                                    {
                                        if (!oSessionHndlr.RequestLines.ContainsKey(header[0].Trim()))
                                        {
                                            oSessionHndlr.RequestLines.Add(header[0].Trim(), header[1].Trim());
                                        }
                                        else
                                        {
                                            oSessionHndlr.RequestLines[header[0]] += header[1].Trim();
                                        }
                                    }
                                    else
                                    {
                                        oSessionHndlr.RequestLines.Add(header[0], string.Empty);
                                    }
                                }
                            }

                            if (oSessionHndlr.RequestLines.ContainsKey("upgrade") && oSessionHndlr.RequestLines["upgrade"] == "websocket")
                            {
                                TCPTunneling.sendRaw(httpCmd, tunnelHostName, ref requestLines, oSessionHndlr.IsSecure, clientStreamReader.BaseStream);
                                return;
                            }
                        }
                        catch (Exception)
                        {
                            return;
                        }
                        //oSessionHndlr.Logger = oclogger;
                        //oSessionHndlr.LuaLogger = luaLogger;

                        oSessionHndlr.ProxyRequest.ServicePoint.Expect100Continue = false;
                        ReadRequestHeaders(ref requestLines, oSessionHndlr);
                        oSessionHndlr.ClientID = clientObj.ClientID;
                        oSessionHndlr.ClientName = clientObj.ClientName;
                        int contentLen = (int)oSessionHndlr.ProxyRequest.ContentLength;

                        oSessionHndlr.ProxyRequest.Headers.Add("MessageFromRequest", "This request was interrupted by a Proxy Service");
                        oSessionHndlr.ProxyRequest.Headers.Add("WindowsUserName", UserName);
                        oSessionHndlr.ProxyRequest.Headers.Add("RequestDateTime", DateTime.Now.ToString());

                        oSessionHndlr.ProxyRequest.AllowAutoRedirect = false;
                        oSessionHndlr.ProxyRequest.AutomaticDecompression = DecompressionMethods.None;

                        oSessionHndlr.RequestURL = oSessionHndlr.ProxyRequest.RequestUri.OriginalString;
                        oSessionHndlr.RequestLength = contentLen;
                        oSessionHndlr.RequestHttpVersion = RequestVersion;
                        try
                        {
                            oSessionHndlr.Port = ((IPEndPoint)clientObj.Client.RemoteEndPoint).Port;
                            oSessionHndlr.HostName = oSessionHndlr.ProxyRequest.Host;
                            oSessionHndlr.IPAddress = Util.DoGetHostEntry(oSessionHndlr.ProxyRequest.Host);
                        }
                        catch (Exception ex)
                        {
                            ////oSessionHndlr.Logger.Logger.Error(ex);
                        }

                        if (oSessionHndlr.Cancel)
                        {
                            if (oSessionHndlr.IsAlive)
                            {
                                ReadLinesInStream(clientStreamReader, requestLines);

                                httpCmd = requestLines.Count > 0 ? requestLines[0] : null;
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }

                        oSessionHndlr.ProxyRequest.ConnectionGroupName = Dns.GetHostEntry(((IPEndPoint)clientObj.Client.RemoteEndPoint).Address).HostName;
                        oSessionHndlr.ProxyRequest.AllowWriteStreamBuffering = true;

                        oSessionHndlr.FinishedRequestEvent = new ManualResetEventSlim(false);
                        oSessionHndlr.RequestHeadersRawData = oSessionHndlr.ProxyRequest.Headers.ToByteArray();

                        try
                        {
                            if (oSessionHndlr.ProxyRequest.Method.ToUpper().CompareTo("POST") == 0 || oSessionHndlr.ProxyRequest.Method.ToUpper().CompareTo("PUT") == 0 || oSessionHndlr.ProxyRequest.Method.ToUpper().CompareTo("OPTIONS") == 0)
                            {
                                oSessionHndlr.ProxyRequest.BeginGetRequestStream(new AsyncCallback(ResponseHandler.GetRequestStreamCallback), oSessionHndlr);
                            }
                            else
                            {
                                oSessionHndlr.RequestEnded = DateTime.UtcNow;
                                oSessionHndlr.ResponseStarted = DateTime.UtcNow;
                              //  AsynInsertHandler.BeginInsertRequest(oSessionHndlr);
                                oSessionHndlr.InsertRequest = new InsertToSqliteDB(AsynInsertHandler.BeginInsertRequest);
                                oSessionHndlr.InsertRequest.BeginInvoke(oSessionHndlr, AsynInsertHandler.EndInsertRequest, oSessionHndlr);
                                oSessionHndlr.ProxyRequest.BeginGetResponse(new AsyncCallback(ResponseHandler.GetResponseCallback), oSessionHndlr);
                            }
                        }
                        catch (Exception ex)
                        {
                            ////oSessionHndlr.Logger.Logger.Error(ex);
                            throw;
                        }

                        oSessionHndlr.FinishedRequestEvent.Wait();

                        if (oSessionHndlr != null)
                        {
                            try
                            {
                                LuaScriptHandler lfun = new LuaScriptHandler();
                                lfun.Execute(oSessionHndlr);

                            }
                            catch (Exception ex)
                            {
                                ////oSessionHndlr.Logger.Logger.Error(ex);
                            }

                            oSessionHndlr.UpdateContentType();
                         //   AsynInsertHandler.BeginInsertResponse(oSessionHndlr);
                            oSessionHndlr.InsertResponse = AsynInsertHandler.BeginInsertResponse;
                            oSessionHndlr.InsertResponse.BeginInvoke(oSessionHndlr, AsynInsertHandler.EndInsertResponse, oSessionHndlr);
                        }

                        httpCmd = null;
                        if (oSessionHndlr.ProxyRequest.KeepAlive)
                        {
                            ReadLinesInStream(clientStreamReader, requestLines);
                            httpCmd = requestLines.FirstOrDefault();//.Count() > 0 ? requestLines[0] : null;
                        }
                        if (oSessionHndlr.ServerResponse != null)
                       {
                            oSessionHndlr.ServerResponse.Close();
                        }

                        if (oSessionHndlr.BlockURL)
                        {
                            if (oSessionHndlr.ClientStream != null)
                            {
                                oSessionHndlr.ClientStream.Close();
                            }
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        ////oSessionHndlr.Logger.Logger.Error(ex);
                        throw;
                    }
                    finally
                    {
                        if (oSessionHndlr != null)
                        {
                            if (oSessionHndlr.ProxyRequest != null)
                            {
                                oSessionHndlr.ProxyRequest.Abort();
                            }
                        }
                    }
                }
                if (oSessionHndlr != null)
                {
                    oSessionHndlr.StopWatch.Stop();
                    //oSessionHndlr.Logger.Logger.Info("Time Taken to complete: " + oSessionHndlr.StopWatch.ElapsedMilliseconds.ToString());

                    try
                    {
                        oSessionHndlr.Close();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                ////TCPClientProcessor.Proxylog.Logger.Error(ex);
            }
            finally
            {
                if (clientStreamReader != null)
                {
                    clientStreamReader.Dispose();
                }

                if (clientStream != null)
                {
                    clientStream.Dispose();
                }
            }
        }

            public static void GetWindowsUserName()
            {
                try
                {
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem"))
                    {
                        using (ManagementObjectCollection collection = searcher.Get())
                        {
                            UserName = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];
                        }
                    }
                }
                catch (Exception ex)
                {
                    ////TCPClientProcessor.Proxylog.Logger.Error(ex);
                    UserName = Environment.UserName;
                }

                if (UserName == null)
                {
                    UserName = Environment.UserName;
                }
            }

        private static void  ReadLinesInStream(CustomBinaryReader clientStreamReader, List<string> requestLines)
        {
            string tmpLine;
            requestLines.Clear();
            while (!String.IsNullOrEmpty(tmpLine = clientStreamReader.ReadLine()))
            {
                requestLines.Add(tmpLine);

            }
           // clientStreamReader.ReadEmpty(null);

        }

        private static void ReadRequestHeaders(ref List<string> requestLines, SessionHandler oSessionHndlr)
        {
            //oSessionHndlr.Logger.Logger.Info("Enter");
            try
            {
                foreach (var header in oSessionHndlr.RequestLines)
                {
                    switch (header.Key.ToLower())
                    {
                        case "accept":
                            oSessionHndlr.ProxyRequest.Accept = header.Value.Trim();
                            break;
                        case "accept-encoding":
                            oSessionHndlr.ProxyRequest.Headers.Add(header.Key.Trim(), header.Value.Trim());
                            break;
                        case "cookie":
                            oSessionHndlr.ProxyRequest.Headers["Cookie"] = header.Value.Trim();
                            break;
                        case "proxy-connection":  
                        case "connection":
                            if (string.Compare(header.Value.Trim(), "keep-alive", true) == 0)
                            {
                                oSessionHndlr.ProxyRequest.KeepAlive = true;
                            }
                            else
                            {
                                oSessionHndlr.ProxyRequest.KeepAlive = false;
                            }
                            break;
                        case "content-length":
                            int contentLen;
                            int.TryParse(header.Value.Trim().Trim(), out contentLen);
                            if (contentLen != 0)
                            {
                                oSessionHndlr.ProxyRequest.ContentLength = contentLen;
                            }
                            break;
                        case "content-type":
                            oSessionHndlr.ProxyRequest.ContentType = header.Value.Trim().Trim();
                            break;
                        case "expect":
                            if (string.Compare(header.Value.Trim(), "100-continue", true) == 0)
                            {
                                oSessionHndlr.ProxyRequest.ServicePoint.Expect100Continue = true;
                            }
                            else
                            {
                                oSessionHndlr.ProxyRequest.Expect = header.Value.Trim();
                            }
                            break;
                        case "host":
                            oSessionHndlr.ProxyRequest.Host = header.Value.Trim();
                            break;
                        case "if-modified-since":
                            String[] sb = header.Value.Split(ConstantVariables.SEMI_SPLIT);
                            DateTime d;
                            if (DateTime.TryParse(sb[0], out d))
                            {
                                oSessionHndlr.ProxyRequest.IfModifiedSince = d;
                            }
                            break;
                        case "range":
                            try
                            {
                                var startEnd = header.Value.Replace(Environment.NewLine, string.Empty).Remove(0, 6).Split(',');

                                foreach (var item in startEnd)
                                {
                                    var indiStartEnd = item.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                                    if (indiStartEnd.Length > 1)
                                    {
                                        if (!String.IsNullOrEmpty(indiStartEnd[1]))
                                            oSessionHndlr.ProxyRequest.AddRange(int.Parse(indiStartEnd[0]), int.Parse(indiStartEnd[1]));
                                        else
                                        {
                                            oSessionHndlr.ProxyRequest.AddRange(int.Parse(indiStartEnd[0]));
                                        }
                                    }
                                    else
                                    {
                                        oSessionHndlr.ProxyRequest.AddRange(int.Parse(indiStartEnd[0]));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                //oSessionHndlr.Logger.Logger.Error("{" + //oSessionHndlr.Logger.ThreadIndex + "}" + oSessionHndlr.ProxyRequest.RequestUri, ex);
                            }
                            break;
                        case "referer":
                            oSessionHndlr.ProxyRequest.Referer = header.Value.Trim();
                            break;
                        case "user-agent":
                            oSessionHndlr.ProxyRequest.UserAgent = header.Value.Trim();
                            break;
                        case "transfer-encoding":
                            if (string.Compare(header.Value, "chunked", true) == 0)
                            {
                                oSessionHndlr.ProxyRequest.SendChunked = true;
                            }
                            else
                            {
                                oSessionHndlr.ProxyRequest.SendChunked = false;
                            }
                            break;
                        case "upgrade":
                            if (string.Compare(header.Value, "HTTP/1.1", true) == 0)
                            {
                                oSessionHndlr.ProxyRequest.Headers.Add(header.Key, header.Value);
                            }
                            break;
                        default:
                            try
                            {
                                if(header.Value != string.Empty)
                                oSessionHndlr.ProxyRequest.Headers.Add(header.Key, header.Value);
                            }
                            catch (Exception ex)
                            {
                                ////oSessionHndlr.Logger.Logger.Error(ex);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                //oSessionHndlr.Logger.Logger.Error(String.Format("{" + //oSessionHndlr.Logger.ThreadIndex + "}" + "Error occurred in adding header in request for URL: ", oSessionHndlr.ProxyRequest.RequestUri), ex);
            }

            //oSessionHndlr.Logger.Logger.Info("Exit");
        }
    }
}
