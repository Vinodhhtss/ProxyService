using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace HTTPProxyServer
{
    public class ResponseHandler
    {
        public static void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            SessionHandler oSessionHndlr = null;
            try
            {
                oSessionHndlr = (SessionHandler)asynchronousResult.AsyncState;

                using (Stream postStream = oSessionHndlr.ProxyRequest.EndGetRequestStream(asynchronousResult))
                {

                    if (oSessionHndlr.ProxyRequest.ContentLength > 0)
                    {
                        oSessionHndlr.ProxyRequest.AllowWriteStreamBuffering = true;
                        try
                        {
                            int totalbytesRead = 0;

                            int bytesToRead;
                            if (oSessionHndlr.ProxyRequest.ContentLength < ConstantVariables.BUFFER_SIZE)
                            {
                                bytesToRead = (int)oSessionHndlr.ProxyRequest.ContentLength;
                            }
                            else
                            {
                                bytesToRead = ConstantVariables.BUFFER_SIZE;
                            }
                            oSessionHndlr.RequestRawData = new byte[(int)oSessionHndlr.ProxyRequest.ContentLength];
                            while (totalbytesRead < (int)oSessionHndlr.ProxyRequest.ContentLength)
                            {
                                var buffer = oSessionHndlr.ClientStreamReader.ReadBytes(bytesToRead);

                                postStream.Write(buffer, 0, buffer.Length);
                                System.Buffer.BlockCopy(buffer, 0, oSessionHndlr.RequestRawData, totalbytesRead, buffer.Length);
                                totalbytesRead += buffer.Length;

                                int RemainingBytes = (int)oSessionHndlr.ProxyRequest.ContentLength - totalbytesRead;
                                if (RemainingBytes < bytesToRead)
                                {
                                    bytesToRead = RemainingBytes;
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            ////oSessionHndlr.Logger.Logger.Error(ex);
                            oSessionHndlr.ProxyRequest.KeepAlive = false;
                            oSessionHndlr.FinishedRequestEvent.Set();
                            return;
                        }
                    }
                    else if (oSessionHndlr.ProxyRequest.SendChunked)
                    {
                        oSessionHndlr.ProxyRequest.AllowWriteStreamBuffering = true;
                        try
                        {
                            StringBuilder sb = new StringBuilder();
                            byte[] byteRead = new byte[1];
                            while (true)
                            {
                                oSessionHndlr.ClientStream.Read(byteRead, 0, 1);
                                sb.Append(Encoding.ASCII.GetString(byteRead));

                                if (sb.ToString().EndsWith(Environment.NewLine))
                                {
                                    var chunkSizeInHex = sb.ToString().Replace(Environment.NewLine, String.Empty);
                                    var chunckSize = int.Parse(chunkSizeInHex, System.Globalization.NumberStyles.HexNumber);
                                    if (chunckSize == 0)
                                    {
                                        for (int i = 0; i < Encoding.ASCII.GetByteCount(Environment.NewLine); i++)
                                        {
                                            oSessionHndlr.ClientStream.ReadByte();
                                        }
                                        break;
                                    }
                                    var totalbytesRead = 0;
                                    int bytesToRead;
                                    if (chunckSize < ConstantVariables.BUFFER_SIZE)
                                    {
                                        bytesToRead = chunckSize;
                                    }
                                    else
                                    {
                                        bytesToRead = ConstantVariables.BUFFER_SIZE;
                                    }

                                    while (totalbytesRead < chunckSize)
                                    {
                                        var buffer = oSessionHndlr.ClientStreamReader.ReadBytes(bytesToRead);

                                        postStream.Write(buffer, 0, buffer.Length);
                                        System.Buffer.BlockCopy(buffer, 0, oSessionHndlr.RequestRawData, totalbytesRead, buffer.Length);
                                        totalbytesRead += buffer.Length;

                                        int RemainingBytes = chunckSize - totalbytesRead;
                                        if (RemainingBytes < bytesToRead)
                                        {
                                            bytesToRead = RemainingBytes;
                                        }
                                    }

                                    for (int i = 0; i < Encoding.ASCII.GetByteCount(Environment.NewLine); i++)
                                    {
                                        oSessionHndlr.ClientStream.ReadByte();
                                    }
                                    sb.Clear();
                                }
                            }
                        }
                        catch (IOException ex)
                        {
                            ////oSessionHndlr.Logger.Logger.Error(ex);

                            oSessionHndlr.ProxyRequest.KeepAlive = false;
                            oSessionHndlr.FinishedRequestEvent.Set();
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (oSessionHndlr == null)
                {
                    return;
                }
                ////oSessionHndlr.Logger.Logger.Error(ex);
                oSessionHndlr.ProxyRequest.KeepAlive = false;
                oSessionHndlr.FinishedRequestEvent.Set();
                return;
            }


           // AsynInsertHandler.BeginInsertRequest(oSessionHndlr);
            oSessionHndlr.InsertRequest = new InsertToSqliteDB(AsynInsertHandler.BeginInsertRequest);
            oSessionHndlr.InsertRequest.BeginInvoke(oSessionHndlr, AsynInsertHandler.EndInsertRequest, oSessionHndlr);

            oSessionHndlr.RequestEnded = DateTime.UtcNow;
            oSessionHndlr.ResponseStarted = DateTime.UtcNow;
            oSessionHndlr.ProxyRequest.BeginGetResponse(new AsyncCallback(ResponseHandler.GetResponseCallback), oSessionHndlr);
        }

        public static void GetResponseCallback(IAsyncResult asynchronousResult)
        {

            SessionHandler oSessionHndlr = (SessionHandler)asynchronousResult.AsyncState;
            //oSessionHndlr.Logger.Logger.Info("Enter");
            try
            {
                oSessionHndlr.ServerResponse = (HttpWebResponse)oSessionHndlr.ProxyRequest.EndGetResponse(asynchronousResult);
                //oSessionHndlr.Logger.Logger.Info("Get response from server");
            }
            catch (WebException webEx)
            {
                oSessionHndlr.ProxyRequest.KeepAlive = false;
                oSessionHndlr.ServerResponse = webEx.Response as HttpWebResponse;
                //oSessionHndlr.Logger.Logger.Error(webEx);
            }

            StreamWriter responseWriter = null;
          //  Stream serverResponseStream = null;
            try
            {
                responseWriter = new StreamWriter(oSessionHndlr.ClientStream);
                responseWriter.AutoFlush = false;

                if (oSessionHndlr.ServerResponse != null)
                {
                    List<Tuple<String, String>> responseHeaders = ProcessResponse(oSessionHndlr.ServerResponse);
                    try
                    {
                        for (int i = 0; i < responseHeaders.Count; i++)
                        {
                            var item = responseHeaders[i];
                            if (oSessionHndlr.ResponseLines.ContainsKey(item.Item1))
                            {
                                oSessionHndlr.ResponseLines[item.Item1] = item.Item2;
                            }
                            else
                            {
                                oSessionHndlr.ResponseLines.Add(item.Item1, item.Item2);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        ////oSessionHndlr.Logger.Logger.Error(ex);
                    }

                    using (BufferedStream serverResponseStream = new BufferedStream(oSessionHndlr.ServerResponse.GetResponseStream()))
                    {
                        if (oSessionHndlr.ServerResponse.Headers.Count == 0 && oSessionHndlr.ServerResponse.ContentLength == -1)
                        {
                            oSessionHndlr.ProxyRequest.KeepAlive = false;
                        }

                        bool isChunked = false;
                        if (oSessionHndlr.ResponseLines.ContainsKey("transfer-encoding"))
                        {
                            if (oSessionHndlr.ResponseLines["transfer-encoding"].ToLower() == "chunked")
                            {
                                isChunked = true;
                            }
                        }

                        if (oSessionHndlr.ResponseLines.ContainsKey("connection"))
                        {
                            if (oSessionHndlr.ResponseLines["connection"].ToLower() == "close")
                            {
                                oSessionHndlr.ProxyRequest.KeepAlive = false;
                            }
                           
                        }
                        else
                        {
                          //  oSessionHndlr.ProxyRequest.KeepAlive = true;
                        }

                        if (oSessionHndlr.ResponseLines.ContainsKey("upgrade"))
                        {
                            oSessionHndlr.upgradeProtocol = oSessionHndlr.ResponseLines["upgrade"];
                        }

                        oSessionHndlr.ResponseHttpVersion = oSessionHndlr.ServerResponse.ProtocolVersion.ToString();
                        oSessionHndlr.StatusCode = (int)oSessionHndlr.ServerResponse.StatusCode;

                        if (oSessionHndlr.ResponseLines.ContainsKey("Location"))
                        {
                            oSessionHndlr.RedirectUrl = oSessionHndlr.ResponseLines["Location"];
                        }
                        oSessionHndlr.ContentEncoding = oSessionHndlr.ServerResponse.ContentEncoding;
                        oSessionHndlr.ContentMimeType = oSessionHndlr.ServerResponse.ContentType;
                        oSessionHndlr.StatusDescription = oSessionHndlr.ServerResponse.StatusDescription;
                        oSessionHndlr.ResponseHeadersRawData = oSessionHndlr.ServerResponse.Headers.ToByteArray();
                      

                        WriteResponseStatus(oSessionHndlr.ServerResponse.ProtocolVersion, oSessionHndlr.ServerResponse.StatusCode, oSessionHndlr.ServerResponse.StatusDescription, responseWriter, oSessionHndlr.Logger);
                        WriteResponseHeaders(responseWriter, responseHeaders, oSessionHndlr.Logger);
                        responseWriter.Flush();
                        if (isChunked)
                        {
                            DataStreamWriter.SendChunked(serverResponseStream, oSessionHndlr.ClientStream, oSessionHndlr);
                        }
                        else
                        {
                            DataStreamWriter.SendNormal(serverResponseStream, oSessionHndlr.ClientStream, oSessionHndlr);
                        }

                        oSessionHndlr.ClientStream.Flush();
                    }
                }
                else
                {
                    oSessionHndlr.ProxyRequest.KeepAlive = false;
                }

            }
            catch (Exception ex)
            {
                ////oSessionHndlr.Logger.Logger.Error(ex);
                oSessionHndlr.ProxyRequest.KeepAlive = false;
            }

            finally
            {
                //if (serverResponseStream != null)
                //{
                //    serverResponseStream.Close();
                //}
             
                if (oSessionHndlr.ServerResponse != null)
                {
                    oSessionHndlr.ServerResponse.Close();
                }

                if (!oSessionHndlr.ProxyRequest.KeepAlive)
                {
                    try
                    {
                        if (responseWriter != null)
                        {
                            responseWriter.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        ////oSessionHndlr.Logger.Logger.Error(ex);
                    }

                    if (oSessionHndlr.ClientStream != null)
                    {
                        oSessionHndlr.ClientStream.Dispose();
                    }
                }
              
                oSessionHndlr.FinishedRequestEvent.Set();
            }

            //Rajesh. Response logging.
            oSessionHndlr.ResponseEnded = DateTime.UtcNow;

            StringBuilder sb = new StringBuilder();
            sb.Append(oSessionHndlr.StartTime);
            sb.Append(", ");
            sb.Append(oSessionHndlr.ResponseEnded.ToBinary().ToString());
            sb.Append(", ");
            sb.Append(oSessionHndlr.ProxyRequest.RequestUri.OriginalString);
            sb.Append(", ");
            sb.Append(oSessionHndlr.StatusCode.ToString());
            sb.Append(", ");
            sb.Append(oSessionHndlr.ProxyRequest.Method);

            //TCPClientProcessor.Proxylog.Logger.Info(sb.ToString());
            //oSessionHndlr.Logger.Logger.Info("Exit");
        }

        private static List<Tuple<String, String>> ProcessResponse(HttpWebResponse response)
        {
            String value = null;
            String header = null;
            List<Tuple<String, String>> returnHeaders = new List<Tuple<String, String>>();
            foreach (String s in response.Headers.Keys)
            {

                if (string.Compare(s, "Set-Cookie", true) == 0)
                {
                    header = s;
                    value = response.Headers[s].Trim();
                }
                //////Rajesh
                else if (string.Compare(s, "content-type", true) == 0)
                {
                    returnHeaders.Add(new Tuple<String, String>(s.ToLower(), response.Headers[s]));
                }
                else
                {
                    returnHeaders.Add(new Tuple<String, String>(s, response.Headers[s]));
                }
            }

            if (!String.IsNullOrWhiteSpace(value))
            {
                response.Headers.Remove(header);
                String[] cookies = ConstantVariables.COOKIE_SPLIT_REGEX.Split(value);

                for (int i = 0; i < cookies.Length; i++)
                {
                    returnHeaders.Add(new Tuple<String, String>("Set-Cookie", cookies[i]));
                }
            }
            return returnHeaders;
        }

        private static void WriteResponseStatus(Version version, HttpStatusCode code, String description, StreamWriter myResponseWriter, CLogger clogger)
        {
            //clogger.Logger.Info("Enter");
            String s = String.Format("HTTP/{0}.{1} {2} {3}", version.Major, version.Minor, (Int32)code, description);
            myResponseWriter.WriteLine(s);
            //clogger.Logger.Info("Exit");
        }

        private static void WriteResponseHeaders(StreamWriter myResponseWriter, List<Tuple<String, String>> headers, CLogger clogger)
        {
            if (headers != null)
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    var header = headers[i];
                    myResponseWriter.WriteLine(String.Format("{0}: {1}", header.Item1, header.Item2));
                }
            }
            myResponseWriter.WriteLine();
        }
    }
}
