using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace HTTPProxyServer
{
    class AsynInsertHandler
    {
        public static void BeginInsertRequest(SessionHandler oSessionHndlr)
        {
            try
            {
                if (oSessionHndlr == null || ProxyServiceManager.Proxydb == null)
                {
                    return;
                }
                MIMETypeIdentifier.GetStreamMIMEType(oSessionHndlr, oSessionHndlr.RequestRawData, true);
                oSessionHndlr.RequestID = ProxyServiceManager.Proxydb.InsertRequestDetails(oSessionHndlr.RequestStarted,
                                  oSessionHndlr.ClientName,
                                  (ushort)oSessionHndlr.ClientID,
                                  oSessionHndlr.HostName,
                                  oSessionHndlr.FirstHeaderLine,
                                  oSessionHndlr.RequestURL,
                                  oSessionHndlr.Method,
                                  oSessionHndlr.IPAddress + ":" + oSessionHndlr.Port,
                                 (int)oSessionHndlr.ThreadIndex,
                                  oSessionHndlr.RequestHeadersRawData,
                                  oSessionHndlr.RequestRawData,
                                  oSessionHndlr.UploadStream.MIME_DLL,
                                  oSessionHndlr.UploadStream.MIME_Signature, oSessionHndlr.RequestEnded);
            }
            catch (Exception ex)
            {
                ////oSessionHndlr.Logger.Logger.Error(ex);
            }
        }

        public static void BeginInsertResponse(SessionHandler oSessionHndlr)
        {
            try
            {
                if (oSessionHndlr == null || ProxyServiceManager.Proxydb == null)
                {
                    return;
                }

                while (oSessionHndlr.RequestID == -1)
                {
                    oSessionHndlr.RequestInsertAttempt++;
                    Thread.Sleep(1000);
                    if (oSessionHndlr.RequestInsertAttempt > 60)
                    {
                        //oSessionHndlr.Logger.Logger.Error("Error in inserting Response");
                        return;
                    }
                }
                if (oSessionHndlr.StatusCode == 206)
                {
                    IsFirstPacket(oSessionHndlr);
                }

                if (oSessionHndlr.ResponseRawData != null && (oSessionHndlr.StatusCode == 200 || oSessionHndlr.IsPartialFirst))
                {
                    MIMETypeIdentifier.GetStreamMIMEType(oSessionHndlr, oSessionHndlr.ResponseRawData, false);
                }
                if (oSessionHndlr != null)
                {
                    ProxyServiceManager.Proxydb.InsertResponseDetails(oSessionHndlr.ResponseStarted, oSessionHndlr.RequestID,
                                   oSessionHndlr.StatusCode,
                                   oSessionHndlr.ContentMimeType,
                                   oSessionHndlr.ResponseHeadersRawData,
                                   oSessionHndlr.ResponseRawData,
                                   oSessionHndlr.DownloadStream.MIME_DLL,
                                   oSessionHndlr.DownloadStream.MIME_Signature, oSessionHndlr.ResponseEnded);
                }
            }
            catch (Exception ex)
            {
                ////oSessionHndlr.Logger.Logger.Error(ex);
            }
        }

        public static void EndInsertResponse(IAsyncResult asynchronousResult)
        {
            SessionHandler oSessionHndlr = null;
            try
            {
                oSessionHndlr = (SessionHandler)asynchronousResult.AsyncState;
            }
            catch (Exception ex)
            {
                ////oSessionHndlr.Logger.Logger.Error(ex);
            }

            if (oSessionHndlr.ResponseRawData != null)
            {
                if (oSessionHndlr.StatusCode == 200 || oSessionHndlr.IsPartialFirst)
                {
                    InsertDownloadDetails(oSessionHndlr);
                    AlertProcessor.ProcessPacket(oSessionHndlr);
                }
            }
        }

        private static bool IsFinalData(SessionHandler oSessionHndlr)
        {
            bool isFinal = false;

            if (oSessionHndlr.ResponseLines.ContainsKey("CONTENT-RANGE"))
            {
                string[] rangeAndLength = oSessionHndlr.ResponseLines["CONTENT-RANGE"].Split(new char[] { '/' });
                string[] ranges = rangeAndLength[0].Split(new char[] { '-' });
                int irange = int.Parse(ranges[1]);
                int iLength = int.Parse(rangeAndLength[1]);
                if (irange == iLength - 1)
                {
                    isFinal = true;
                }
            }

            return isFinal;

        }

        private static void IsFirstPacket(SessionHandler oSessionHndlr)
        {
            if (oSessionHndlr.ResponseLines.ContainsKey("CONTENT-RANGE"))
            {
                string[] rangeAndLength = oSessionHndlr.ResponseLines["CONTENT-RANGE"].Split(new char[] { '/' });
                string[] ranges = rangeAndLength[0].Split(new char[] { ' ', '-' });
                int irange = -1;
                if (ranges.Length == 3)
                {
                    irange = int.Parse(ranges[1]);
                }
                else
                {
                    irange = int.Parse(ranges[0]);
                }
                if (irange == 0)
                {
                    oSessionHndlr.IsPartialFirst = true;
                }
            }
        }

        public static void EndInsertRequest(IAsyncResult asynchronousResult)
        {
            SessionHandler oSessionHndlr = null;
            try
            {
                oSessionHndlr = (SessionHandler)asynchronousResult.AsyncState;
            }
            catch (Exception ex)
            {
                ////oSessionHndlr.Logger.Logger.Error(ex);
            }

            if (oSessionHndlr.RequestRawData != null)
            {
                InsertUploadDetails(oSessionHndlr);
                AlertProcessor.ProcessPacket(oSessionHndlr);
            }
        }

        private static void InsertUploadDetails(SessionHandler oSessionHndlr)
        {
            try
            {
                string md5 = CalculateMD5Hash(oSessionHndlr.RequestRawData);
                ProxyServiceManager.Proxydb.InsertUploadDetails(oSessionHndlr.RequestID, md5,
                                                                oSessionHndlr.UploadStream.Signer, oSessionHndlr.UploadStream.Version);
            }
            catch (Exception ex)
            {
                ////oSessionHndlr.Logger.Logger.Error(ex);
            }

        }
        private static void InsertDownloadDetails(SessionHandler oSessionHndlr)
        {
            try
            {
                string md5 = string.Empty;
                md5 = CalculateMD5Hash(oSessionHndlr.ResponseRawData);

                ProxyServiceManager.Proxydb.InsertDownloadDetails(oSessionHndlr.RequestID, md5,
                                                                oSessionHndlr.DownloadStream.Signer, oSessionHndlr.DownloadStream.Version, oSessionHndlr.IsPartialFirst);
            }
            catch (Exception ex)
            {
                ////oSessionHndlr.Logger.Logger.Error(ex);
            }

        }

        private static string CalculateMD5Hash(byte[] inputBytes)
        {
            StringBuilder sb = new StringBuilder(string.Empty);
            if (inputBytes != null)
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] hash = md5.ComputeHash(inputBytes);


                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("X2"));
                }
            }
            return sb.ToString();
        }
    }
}
