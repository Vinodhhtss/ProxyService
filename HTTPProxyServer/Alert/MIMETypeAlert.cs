using System.Collections.Generic;

namespace HTTPProxyServer
{
    public class MIMETypeAlert : IAlert
    {
        public static List<string> MIMEEntries = new List<string>();

        static MIMETypeAlert()
        {
            MIMEEntries.Add("application/x-msdownload");
            MIMEEntries.Add("application/pdf");
            MIMEEntries.Add("application/msword");
        }

        public bool IsAlert(SessionHandler oSessionHndlr)
        {
            if (MIMEEntries.Contains(oSessionHndlr.DownloadStream.MIME_DLL) || MIMEEntries.Contains(oSessionHndlr.DownloadStream.MIME_Signature))
            {
                return true;
            }
            return false;
        }
    }
}
