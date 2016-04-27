using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace HTTPProxyServer
{
    public delegate void InsertToSqliteDB(SessionHandler oSession);

    public class StreamDetails
    {
        public string MIME_DLL { get; set; }
        public string MIME_Signature { get; set; }
        public string MD5 { get; set; }
        public string Signer { get; set; }
        public string Version { get; set; }
    }

    public class SessionHandler
    {
        public CustomBinaryReader ClientStreamReader { get; set; }
        public Stream ClientStream { get; set; }

        //json variables
        public string HostName { get; set; }
        public string FirstHeaderLine { get; set; }

        public string RequestURL { get; set; }
        public string RequestHttpVersion { get; set; }
        public DateTime RequestStarted { get; set; }
        public DateTime RequestEnded { get; set; }
        public DateTime ResponseStarted { get; set; }
        public DateTime ResponseEnded { get; set; }
        public string Method { get; set; }
        public int RequestLength { get; set; }
        public int Port { get; set; }
        public string IPAddress { get; set; }
        public string RedirectUrl { get; set; }
        public string ContentEncoding { get; set; }
        public string ContentMimeType { get; set; }
        public long ResponseBodySize { get; set; }
        public long ResponseHeadersSize { get; set; }

        public string ResponseHttpVersion { get; set; }
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }

        public bool IsAlive { get; set; }
        public bool Cancel { get; set; }
        public bool IsSecure { get; set; }

        public bool BlockURL { get; set; }
        public bool FilterImg { get; set; }

        public HttpWebResponse ServerResponse { get; set; }
        public HttpWebRequest ProxyRequest { get; set; }

        public ManualResetEventSlim FinishedRequestEvent { get; set; }
        public string upgradeProtocol { get; set; }
        public CLogger Logger { get; set; }
        public CLogger LuaLogger { get; set; }

        //Rajesh. Req Log
        public String StartTime { get; set; }
        public Stopwatch StopWatch { get; set; }


        public long ThreadIndex { get; set; }
        public Dictionary<string, string> RequestLines { get; set; }
        public Dictionary<string, string> ResponseLines { get; set; }

        public int ClientID { get; set; }
        public string ClientName { get; set; }

        public byte[] RequestRawData = null;
        public byte[] RequestHeadersRawData = null;
        public byte[] ResponseRawData = null;
        public byte[] ResponseHeadersRawData = null;

        public StreamDetails UploadStream;
        public StreamDetails DownloadStream;

        public int RequestID = -1;
        public int RequestInsertAttempt = 0;
        public InsertToSqliteDB InsertRequest;
        public InsertToSqliteDB InsertResponse;

        public bool IsPartialFirst = false;
        public SessionHandler(long threadIndex)
        {
            ThreadIndex = threadIndex;
            RequestLines = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            ResponseLines = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            StopWatch = new Stopwatch();
            RedirectUrl = string.Empty;

            UploadStream = new StreamDetails();
            DownloadStream = new StreamDetails();
        }

        public void Close()
        {
            CleanUpLoggers();
        }

        public void CleanUpLoggers()
        {
            if (Logger != null)
            {
                //Logger.Cleanup();
            }
            if (LuaLogger != null)
            {
                //LuaLogger.Cleanup();
            }
        }

        public void UpdateContentType()
        {
            string val = null;
            if (ResponseLines.TryGetValue("content-type", out val) == true)
            {
                if (val != null)
                {
                    if (val.Contains("image") == true)
                    {
                        FilterImg = true;
                    }
                    val = null;
                }
            }
        }
    }
}
