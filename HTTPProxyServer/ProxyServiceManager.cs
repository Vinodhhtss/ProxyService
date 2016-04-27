using CertificateManagement;
using System;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPProxyServer
{
    public struct TcpClientObject
    {
        public Socket Client;
        public int ClientID;
        public string ClientName;
        public int ThreadIndex;

        public TcpClientObject(Socket client, int clientID, string clientName)
        {
            Client = client;
            ClientID = clientID;
            ClientName = clientName;
            ThreadIndex = 0;
        }
    }

    public class ProxyServiceManager
    {
        private static readonly ProxyServiceManager m_server = new ProxyServiceManager();

        private static Socket m_tcpListener;

        private static Thread m_listenerThread;
        private static Thread m_alertThread;

        private static ManualResetEventSlim m_tcpClientConnected = new ManualResetEventSlim(false);

        private static int m_threadIndex = 0;
        private static ProxyDbs.ProxyDb m_proxydb;

        public static ProxyDbs.ProxyDb Proxydb
        {
            get { return ProxyServiceManager.m_proxydb; }
        }

        public static ProxyServiceManager Server
        {
            get
            {
                return m_server;
            }
        }

        private ProxyServiceManager()
        {
            // m_tcpListener = new TcpListener(ListeningIPInterface, ListeningPort);
            IPEndPoint endPt = new IPEndPoint(IPAddress.Loopback, 8871);
            m_tcpListener = new Socket(endPt.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            m_tcpListener.Bind(endPt);
            m_tcpListener.Listen(50);
            m_tcpListener.NoDelay = true;
            try
            {
                m_proxydb = new ProxyDbs.ProxyDb(true);
            }
            catch (Exception ex)
            {
                ////TCPClientProcessor.Proxylog.Logger.Error(ex);
            }
        }

        public IPAddress ListeningIPInterface
        {
            get
            {
                IPAddress addr = IPAddress.Loopback;
                if (ConfigurationManager.AppSettings["ListeningIPInterface"] != null)
                {
                    IPAddress.TryParse(ConfigurationManager.AppSettings["ListeningIPInterface"], out addr);
                }

                return addr;
            }
        }

        public Int32 ListeningPort
        {
            get
            {
                Int32 port = 8872;
                if (ConfigurationManager.AppSettings["ListeningPort"] != null)
                {
                    Int32.TryParse(ConfigurationManager.AppSettings["ListeningPort"], out port);
                }

                return port;
            }
        }

        public bool Start()
        {
            while (true)
            {
                try
                {
                    if (Util.ReadProxyConfig())
                    {
                        break;
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }
            // CertMaker.CreateAndExportRootCert(string.Empty);
            //  CertMaker.RemoveGeneratedPersonalCerts();
            //  m_tcpListener.Start();

            m_listenerThread = new Thread(new ParameterizedThreadStart(Listen));
            m_listenerThread.Start(m_tcpListener);

            m_alertThread = new Thread(AlertProcessor.AlertQueueProcessor);
            m_alertThread.Start();

            //ServicePointManager.DefaultConnectionLimit = 200;
            // ServicePointManager.MaxServicePointIdleTime = 500;
            //  ServicePointManager.UseNagleAlgorithm = false;
            // ServicePointManager.SetTcpKeepAlive(false, 0, 0);
            // System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            //HttpWebRequest certificate validation callback
            // ServicePointManager.ServerCertificateValidationCallback = CertificateValidationCallBack;
            //ServicePointManager.CheckCertificateRevocationList = true;
            ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
             {
                 //if (sslPolicyErrors == SslPolicyErrors.None)
                 {
                     return true;
                 }
                 //else
                 //{
                 //    return false;
                 //}
             };
            return true;
        }

        private static bool CertificateValidationCallBack(object sender,
       System.Security.Cryptography.X509Certificates.X509Certificate certificate,
       System.Security.Cryptography.X509Certificates.X509Chain chain,
       System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }
            // If there are errors in the certificate chain, look at each error to determine the cause.
            if ((sslPolicyErrors & System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                if (chain != null && chain.ChainStatus != null)
                {
                    foreach (System.Security.Cryptography.X509Certificates.X509ChainStatus status in chain.ChainStatus)
                    {
                        if ((certificate.Subject == certificate.Issuer) &&
                           (status.Status == System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.UntrustedRoot))
                        {
                            // Self-signed certificates with an untrusted root are valid.
                            continue;
                        }
                        else
                        {
                            if (status.Status != System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.NoError)
                            {
                                // If there are any other errors in the certificate chain, the certificate is invalid,
                                // so the method returns false.
                                return false;
                            }
                        }
                    }
                }
                // When processing reaches this line, the only errors in the certificate chain are
                // untrusted root errors for self-signed certificates. These certificates are valid
                // for default Exchange Server installations, so return true.
                return true;
            }
            else
            {
                // In all other cases, return false.
                return false;
            }
        }


        public void Stop()
        {
            m_tcpListener.Close();

            //wait for server to finish processing current connections...

            m_listenerThread.Abort();
            m_alertThread.Abort();
            m_listenerThread.Join();
            m_alertThread.Join();

            //m_proxydb.DeinitProxyDb();

        }

        public static void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            // Get the listener that handles the client request.
            Socket listener = (Socket)ar.AsyncState;

            // End the operation and display the received data on  
            // the console.

            Socket client = listener.EndAccept(ar);
            //    client.NoDelay = true;
            //client.Client.NoDelay = true;

            TcpClientObject tcp = TcpHelperUtil.GetPortDetails(client);
            tcp.Client = client;
            try
            {
                tcp.ThreadIndex = ++m_threadIndex;
            }
            catch (Exception ex)
            {
                ////TCPClientProcessor.Proxylog.Logger.Error(ex);
                tcp.ThreadIndex = m_threadIndex = 0;
            }
            // Signal the calling thread to continue.
            m_tcpClientConnected.Set();
            try
            {
                Task.Factory.StartNew(() => TCPClientProcessor.ProcessTCPClient(tcp));
            }
            catch (Exception ex)
            {
                ////TCPClientProcessor.Proxylog.Logger.Error(ex);
            }

        }

        private static void Listen(Object obj)
        {

            Socket listener = (Socket)obj;
            try
            {
                while (true)
                {
                    m_tcpClientConnected.Reset();

                    listener.BeginAccept(new AsyncCallback(DoAcceptTcpClientCallback), listener);
                    // Wait until a connection is made and processed before  
                    // continuing.
                    m_tcpClientConnected.Wait();
                }
            }
            catch (Exception ex)
            {
                ////TCPClientProcessor.Proxylog.Logger.Error(ex);
            }
            finally
            {
            }
        }
    }
}
