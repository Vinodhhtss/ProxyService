using System;
using System.Text;

namespace HTTPProxyServer
{
    public class MessagesDecoder
    {
        public static string DecodeMessage(SessionHandler oSessionHndlr, bool IsReponse)
        {
            string temp = string.Empty;
            try
            {
                if (IsReponse)
                {
                    if (oSessionHndlr.ResponseRawData != null)
                    {
                        temp = Encoding.UTF8.GetString(oSessionHndlr.ResponseRawData);
                    }
                }
                else
                {
                    if (oSessionHndlr.RequestRawData != null)
                    {
                        temp = Encoding.UTF8.GetString(oSessionHndlr.RequestRawData);
                    }
                }
            }
            catch (Exception ex)
            {
                ////TCPClientProcessor.Proxylog.Logger.Error(ex);
            }
            return temp;
        }
    }
}

