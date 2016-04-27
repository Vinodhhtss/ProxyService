using System;
using System.IO;
using System.Text;

namespace HTTPProxyServer
{
    public partial class DataStreamWriter
    {
        public static void SendNormal(Stream inStream, Stream outStream, SessionHandler oSessionHndlr)
        {
            if (!outStream.CanWrite)
                return;
            if (inStream.CanRead)
            {
                Byte[] buffer = new Byte[ConstantVariables.BUFFER_SIZE];
                int bytesRead;
                int totalBytes = 0;
                using (MemoryStream ms = new MemoryStream())
                {
                    while ((bytesRead = inStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        totalBytes += bytesRead;
                        outStream.Write(buffer, 0, bytesRead);
                        ms.Write(buffer, 0, bytesRead);
                    }
                    oSessionHndlr.ResponseBodySize = totalBytes;
                    if (totalBytes > 0)
                    {
                        oSessionHndlr.ResponseRawData = ms.ToArray();
                    }
                }
            }
        }

        public static void SendChunked(Stream inStream, Stream outStream, SessionHandler oSessionHndlr)
        {
            if (!outStream.CanWrite)
                return;
            Byte[] buffer = new Byte[ConstantVariables.BUFFER_SIZE];

            int totalBytes = 0;
            var ChunkTrail = Encoding.UTF8.GetBytes(Environment.NewLine);

            int bytesRead;
            using (MemoryStream ms = new MemoryStream())
            {
                while ((bytesRead = inStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    totalBytes += bytesRead;
                    var ChunkHead = Encoding.UTF8.GetBytes(bytesRead.ToString("x2"));
                    outStream.Write(ChunkHead, 0, ChunkHead.Length);
                    outStream.Write(ChunkTrail, 0, ChunkTrail.Length);
                    outStream.Write(buffer, 0, bytesRead);
                    ms.Write(buffer, 0, bytesRead);

                    outStream.Write(ChunkTrail, 0, ChunkTrail.Length);
                }
                var ChunkEnd = Encoding.UTF8.GetBytes(0.ToString("x2") + Environment.NewLine + Environment.NewLine);
                oSessionHndlr.ResponseBodySize = totalBytes;
                outStream.Write(ChunkEnd, 0, ChunkEnd.Length);
                if (totalBytes > 0)
                {
                    oSessionHndlr.ResponseRawData = ms.ToArray();
                }
            }
        }

        public static byte[] CombineByteArray(byte[] previous, byte[] newone)
        {
            if (previous == null && newone == null)
            {
                return null;
            }
            byte[] merged = null;
            try
            {
                if (previous == null)
                {
                    merged = new byte[newone.Length];

                    System.Buffer.BlockCopy(newone, 0, merged, 0, newone.Length);
                }
                else
                {
                    merged = new byte[previous.Length + newone.Length];

                    System.Buffer.BlockCopy(previous, 0, merged, 0, previous.Length);
                    System.Buffer.BlockCopy(newone, 0, merged, previous.Length, newone.Length);
                }
            }
            catch (Exception ex)
            {
                ////TCPClientProcessor.Proxylog.Logger.Error(ex);
                return merged;
            }
            return merged;
        }
    }
}
