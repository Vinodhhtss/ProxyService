using System;
using System.Collections.Generic;

namespace HTTPDataAnalyzer
{
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

        public long ThreadIndex { get; set; }
        public Dictionary<string, string> RequestLines { get; set; }
        public Dictionary<string, string> ResponseLines { get; set; }

        public int ClientID { get; set; }
        public string ClientName { get; set; }

        public byte[] RequestRawData = null;
        public byte[] RequestHeadersRawData = null;
        public byte[] ResponseRawData = null;
        public byte[] ResponseHeadersRawData = null;

        public List<byte[]> Files = new List<byte[]>();

        public string FileName = string.Empty;
        public string FileMIMETypeDLL = string.Empty;
        public string FileMIMETypeSignature = string.Empty;

        public CLogger LuaLogger { get; set; }

        //Temp Error Reporting
        public int RequestID { get; set; }

        public StreamDetails UploadStream;
        public StreamDetails DownloadStream;

        public SessionHandler()
        {
            ThreadIndex = -1;
            ResponseLines = new Dictionary<string, string>();
            RequestLines = new Dictionary<string, string>();
            RequestRawData = new byte[0];
            RequestHeadersRawData = new byte[0];
            ResponseRawData = new byte[0];
            ResponseHeadersRawData = new byte[0];
            RedirectUrl = string.Empty;
        }
    }
}
