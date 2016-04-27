namespace HTTPDataAnalyzer
{
    public class PDFIdentifier : IConditionChecker
    {
        public string MIMEType = "application/pdf";
      
        public bool CheckCondition(SessionHandler oSessionHandler)
        {
            if (oSessionHandler.DownloadStream.MIME_DLL == MIMEType || oSessionHandler.DownloadStream.MIME_Signature == MIMEType)
            {
                return true;
            }
            else if (oSessionHandler.UploadStream.MIME_Signature == MIMEType || oSessionHandler.UploadStream.MIME_DLL == MIMEType)
            {
                return true;
            }
            return false;
        }

        //public void SendMessage(SessionHandler oSessionHandler)
        //{
        //    TCPClients.SendAlertMessageToServer("1", oSessionHandler, null);
        //}       
    }
}
