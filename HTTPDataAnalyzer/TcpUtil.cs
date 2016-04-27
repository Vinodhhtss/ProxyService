//using log4net;
using System;
using System.IO;
using System.Text;

namespace HTTPDataAnalyzer
{
    class TcpUtil
    {
        public static string ReadDataJob(Stream clientStream, out bool keepAlive)
        {
            keepAlive = false;
            StringBuilder sb = new StringBuilder();
            string result = string.Empty;
            int bytesRead = 0;
            int totalBytesReceived = 0;
            byte[] receivedBytes = new byte[8192];

            try
            {
                while ((bytesRead = clientStream.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
                {
                    totalBytesReceived += bytesRead;
                    sb.Append(Encoding.ASCII.GetString(receivedBytes, 0, bytesRead));
                    if (sb.ToString().EndsWith("<EOF>"))
                    {
                        result = sb.ToString().TrimEnd("<EOF>".ToCharArray());
                        break;
                    }
                    else if (sb.ToString().EndsWith("keepalive"))
                    {
                        result = sb.ToString().TrimEnd("keepalive".ToCharArray());
                        keepAlive = true;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                //HTTPDataAnalyzer.Poll.//JobsSearcher.Logger.Error(ex);
            }
            return result;
        }

        public static string ReadData(Stream clientStream)
        {
            StringBuilder sb = new StringBuilder();
            string result = string.Empty;
            int bytesRead = 0;
            int totalBytesReceived = 0;
            byte[] receivedBytes = new byte[8192];

            try
            {
                while ((bytesRead = clientStream.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
                {
                    totalBytesReceived += bytesRead;
                    sb.Append(Encoding.ASCII.GetString(receivedBytes, 0, bytesRead));
                    if (sb.ToString().EndsWith("<EOF>"))
                    {
                        result = sb.ToString().Replace("<EOF>", string.Empty);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                //logger.Error(ex);
            }
            return result;
        }

        public static byte[] ReadDataAsBytes(Stream clientStream)
        {
            StringBuilder sb = new StringBuilder();
            string result = string.Empty;
            int bytesRead = 0;
            byte[] receivedBytes = new byte[8192];

            try
            {
               // string tempEnd = Convert.ToBase64String(Encoding.ASCII.GetBytes("<EOF>"));
                while ((bytesRead = clientStream.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
                {
                    Array.Resize(ref receivedBytes, bytesRead);
                    break;
                }
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
            }

            return receivedBytes;
        }

        public static void WriteHeaderData(Stream clientStream, byte[] message)
        {
            clientStream.Write(message, 0, message.Length);
            byte[] output = Encoding.ASCII.GetBytes("<EOF>");
            clientStream.Write(output, 0, output.Length);
            clientStream.Flush();
        }

        public static void WriteData(Stream clientStream, byte[] message)
        {
            clientStream.Write(message, 0, message.Length);
            byte[] output = Encoding.ASCII.GetBytes("<EOF>");
            clientStream.Write(output, 0, output.Length);
            clientStream.Flush();
        }
    }
}
