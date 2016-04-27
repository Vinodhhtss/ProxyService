using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace HTTPProxyServer
{
    public static class TCPTunneling
    {
        public static void sendRaw(string hostname, int tunnelPort, System.IO.Stream clientStream)
        {
            using (System.Net.Sockets.TcpClient tunnelClient = new System.Net.Sockets.TcpClient(hostname, tunnelPort))
            {
                using (var tunnelStream = tunnelClient.GetStream())
                {
                    var tunnelReadBuffer = new byte[ConstantVariables.BUFFER_SIZE];

                    Task sendRelay = new Task(() => CopyTo(clientStream, tunnelStream, ConstantVariables.BUFFER_SIZE));
                    Task receiveRelay = new Task(() => CopyTo(tunnelStream, clientStream, ConstantVariables.BUFFER_SIZE));

                    sendRelay.Start();
                    receiveRelay.Start();

                    Task.WaitAll(sendRelay, receiveRelay);
                }
            }
        }

        public static void sendRaw(string httpCmd, string secureHostName, ref List<string> requestLines, bool isSecure, Stream clientStream)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(httpCmd);
            sb.Append(Environment.NewLine);

            string hostname = secureHostName;
            for (int i = 1; i < requestLines.Count; i++)
            {
                var header = requestLines[i];

                if (secureHostName == null)
                {
                    String[] headerParsed = header.Split(ConstantVariables.COLON_SPACE_SPLIT, 2, StringSplitOptions.None);
                    switch (headerParsed[0].ToLower())
                    {
                        case "host":
                            var hostdetail = headerParsed[1];
                            if (hostdetail.Contains(ConstantVariables.COLON))
                            {
                                hostname = hostdetail.Split(ConstantVariables.COLON)[0].Trim();
                            }
                            else
                            {
                                hostname = hostdetail.Trim();
                            }
                            break;
                        default:
                            break;
                    }
                    headerParsed = null;
                }
                sb.AppendLine(header);
            }
            sb.Append(Environment.NewLine);

            int tunnelPort = 80;
            if (isSecure)
            {
                tunnelPort = 443;
            }

            System.Net.Sockets.TcpClient tunnelClient = new System.Net.Sockets.TcpClient(hostname, tunnelPort);
            var tunnelStream = tunnelClient.GetStream() as System.IO.Stream;

            if (isSecure)
            {
                var sslStream = new SslStream(tunnelStream);
                sslStream.AuthenticateAsClient(hostname);
                tunnelStream = sslStream;
            }

            var sendRelay = new Task(() => CopyTo(sb.ToString(), clientStream, tunnelStream, ConstantVariables.BUFFER_SIZE));
            var receiveRelay = new Task(() => CopyTo(tunnelStream, clientStream, ConstantVariables.BUFFER_SIZE));

            sendRelay.Start();
            receiveRelay.Start();

            Task.WaitAll(sendRelay, receiveRelay);

            if (tunnelStream != null)
            {
                tunnelStream.Close();
            }

            if (tunnelClient != null)
            {
                tunnelClient.Close();
            }
        }

        public static void CopyTo(this Stream input, Stream output)
        {
            input.CopyTo(output, ConstantVariables.BUFFER_SIZE);
            return;
        }

        public static void CopyTo(string initialData, Stream input, Stream output, int bufferSize)
        {
            var bytes = Encoding.UTF8.GetBytes(initialData);
            output.Write(bytes, 0, bytes.Length);
            CopyTo(input, output, bufferSize);
        }

        public static void CopyTo(this Stream input, Stream output, int bufferSize)
        {
            try
            {
                if (!input.CanRead)
                {
                    throw new InvalidOperationException("input must be open for reading");
                }

                if (!output.CanWrite)
                {
                    throw new InvalidOperationException("output must be open for writing");
                }

                byte[][] buf = { new byte[bufferSize], new byte[bufferSize] };
                int[] bufl = { 0, 0 };
                int bufno = 0;
                IAsyncResult read = input.BeginRead(buf[bufno], 0, buf[bufno].Length, null, null);
                IAsyncResult write = null;

                while (true)
                {
                    read.AsyncWaitHandle.WaitOne();
                    bufl[bufno] = input.EndRead(read);


                    if (bufl[bufno] == 0)
                    {
                        break;
                    }

                    if (write != null)
                    {
                        write.AsyncWaitHandle.WaitOne();
                        output.EndWrite(write);
                    }

                    write = output.BeginWrite(buf[bufno], 0, bufl[bufno], null, null);

                    bufno ^= 1; // bufno = (bufno == 0 ? 1 : 0);
                    read = input.BeginRead(buf[bufno], 0, buf[bufno].Length, null, null);

                }

                if (write != null)
                {
                    write.AsyncWaitHandle.WaitOne();
                    output.EndWrite(write);
                }

                output.Flush();
            }
            catch (Exception ex)
            {
                ////TCPClientProcessor.Proxylog.Logger.Error(ex);
            }
            return;
        }
    }
}
