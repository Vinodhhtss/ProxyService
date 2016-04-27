using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace HTTPProxyServer
{
    public class CustomBinaryReader : BinaryReader
    {
        public CustomBinaryReader(Stream stream, Encoding encoding)
            : base(stream, encoding)
        {

        }
     
        public string ReadLine()
        {
            char[] buf = new char[1];
            StringBuilder _readBuffer = new StringBuilder();
            try
            {
                //   var charsRead = 0;
                char lastChar = new char();

                while (base.Read(buf, 0, 1) > 0)
                {
                    if (lastChar == '\r' && buf[0] == '\n')
                    {
                        return _readBuffer.Remove(_readBuffer.Length - 1, 1).ToString();
                    }
                    else
                        if (buf[0] == '\0')
                        {
                            return _readBuffer.ToString();
                        }
                        else
                        {
                            _readBuffer.Append(buf[0]);
                        }

                    lastChar = buf[0];
                }
                return _readBuffer.ToString();
            }
            catch (Exception ex)
            {
                //if (logger == null)
                {
                    ////TCPClientProcessor.Proxylog.Logger.Error(ex);
                }
                //else
                {
                    //logger.Logger.Error(ex);
                }
                return _readBuffer.ToString();
            }
        }
    }
}
