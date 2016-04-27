using System.Net;

namespace HTTPProxyServer
{
    public class ProxyConfig
    {
        public static UpStreamProxy UpStream = null;
        public static WebProxy UpStreamWebProxy = null;

        public static void AssignWebProxy()
        {
            UpStreamWebProxy = new WebProxy(UpStream.IPAddress, UpStream.Port);
        }
    }

    public class UpStreamProxy
    {
        public string IPAddress { get; set; }
        public int Port { get; set; }
    }
}
