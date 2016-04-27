using System;
using System.Text;

namespace HTTPDataAnalyzer
{
    public class MessagesDecoder
    {
        public static string MessageDecoderRequest(SessionHandler oSessionHndlr)
        {
            try
            {
                return Encoding.UTF8.GetString(oSessionHndlr.RequestRawData);
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
            }
            return string.Empty;
        }

        public static string MessageDecoderResponse(SessionHandler oSessionHndlr)
        {
            try
            {
                return Encoding.UTF8.GetString(oSessionHndlr.ResponseRawData);
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
            }
            return string.Empty;
        }
    }
}

