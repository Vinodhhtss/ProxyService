using System;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace HTTPDataAnalyzer
{
    public class StateObject
    {
        public Socket ClientSocket = null;
        public byte[] buffer = new byte[8192];
        public SslStream ClientStream = null;

        public StateObject()
        {
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void Close()
        {
            try
            {
                if (ClientStream != null)
                {
                    ClientStream.Dispose();
                }
                if (ClientSocket != null)
                {
                    ClientSocket.Dispose();
                }
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
            }
        }
    }

    class TCPSocket
    {
        public static void Connect(StateObject client)
        {
            IPEndPoint endPint = new IPEndPoint(IPAddress.Parse(TCPClients.ListeningIPInterface), TCPClients.ListeningPort);
            client.ClientSocket.Connect(endPint);
            client.ClientStream = new SslStream(new NetworkStream(client.ClientSocket), false);
            client.ClientStream.ReadTimeout = 60000;

        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate,
                                                      X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static void ConnectIgnoreCertError(StateObject client)
        {
            IPEndPoint endPint = new IPEndPoint(IPAddress.Parse(TCPClients.ListeningIPInterface), TCPClients.ListeningPort);
            client.ClientSocket.Connect(endPint);
            client.ClientStream = new SslStream(new NetworkStream(client.ClientSocket), false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
            client.ClientStream.ReadTimeout = 60000;
        }
    }
}
