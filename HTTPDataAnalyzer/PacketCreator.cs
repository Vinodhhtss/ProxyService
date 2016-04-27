using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace HTTPDataAnalyzer
{
    public class PacketCreator
    {
        public static Queue<SessionHandler> CreatePackets(DataTable dt)
        {
            Queue<SessionHandler> tempBuffer = new Queue<SessionHandler>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow row = dt.Rows[i];

                SessionHandler oSessionHandler = new SessionHandler();
                try
                {
                    oSessionHandler.RequestStarted = Convert.ToDateTime(row["start_time"]);
                    oSessionHandler.ClientName = Convert.ToString(row["process_name"]);
                    oSessionHandler.ClientID = Convert.ToInt32(row["process_id"]);
                    oSessionHandler.HostName = Convert.ToString(row["host_name"]);
                    oSessionHandler.FirstHeaderLine = Convert.ToString(row["first_header_line"]);
                    oSessionHandler.RequestURL = Convert.ToString(row["url"]);
                    oSessionHandler.Method = Convert.ToString(row["method"]);
                    oSessionHandler.IPAddress = Convert.ToString(row["server_ip"]);
                    oSessionHandler.ThreadIndex = Convert.ToInt32(row["thread_index"]);
                    oSessionHandler.RequestHeadersRawData = Convert.FromBase64String(Convert.ToString(row["request_headers"]));
                    oSessionHandler.RequestRawData = Convert.FromBase64String(Convert.ToString(row["request_body"]));

                    oSessionHandler.UploadStream = new StreamDetails();
                    oSessionHandler.UploadStream.MIME_DLL = Convert.ToString(row["upload_mime_dll"]);
                    oSessionHandler.UploadStream.MIME_Signature = Convert.ToString(row["upload_mime_singature"]);
                    oSessionHandler.RequestEnded = Convert.ToDateTime(row["request_end"]);

                    oSessionHandler.RequestID = Convert.ToInt32(row["request_id"]);
                    oSessionHandler.ResponseStarted = Convert.ToDateTime(row["request_end"]);
                    oSessionHandler.StatusCode = Convert.ToInt32(row["status_code"]);
                    oSessionHandler.ContentMimeType = Convert.ToString(row["content_type"]);
                    oSessionHandler.ResponseHeadersRawData = Convert.FromBase64String(Convert.ToString(row["response_headers"]));
                    oSessionHandler.ResponseRawData = Convert.FromBase64String(Convert.ToString(row["response_body"]));

                    oSessionHandler.DownloadStream = new StreamDetails();
                    oSessionHandler.DownloadStream.MIME_DLL = Convert.ToString(row["download_mime_dll"]);
                    oSessionHandler.DownloadStream.MIME_Signature = Convert.ToString(row["download_mime_singature"]);
                    oSessionHandler.ResponseEnded = Convert.ToDateTime(row["request_end"]);

                    ReadHeader(oSessionHandler);

                    if (oSessionHandler.ResponseRawData.Length > 0)
                    {
                        MIMETypeIdentifier.GetStreamMIMEType(oSessionHandler, oSessionHandler.ResponseRawData, false);
                    }
                }
                catch (Exception ex)
                {
                    //AnalyzerManager.Logger.Error(ex);
                }
                tempBuffer.Enqueue(oSessionHandler);
            }

            return tempBuffer;
        }

        private static void ReadHeader(SessionHandler oSessionHandler)
        {
            try
            {
                if (oSessionHandler.RequestHeadersRawData != null && oSessionHandler.RequestHeadersRawData.Length > 0)
                {
                    SplitHeader(oSessionHandler, Encoding.UTF8.GetString(oSessionHandler.RequestHeadersRawData), true);
                }

                if (oSessionHandler.ResponseHeadersRawData != null && oSessionHandler.ResponseHeadersRawData.Length > 0)
                {
                    SplitHeader(oSessionHandler, Encoding.UTF8.GetString(oSessionHandler.ResponseHeadersRawData), false);
                }
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
            }
        }

        private static void SplitHeader(SessionHandler oSessionHandler, string headers, bool isRequest)
        {
            try
            {
                string[] headersSplit = headers.Replace("\r\n", "\n").Split(new char[]
			{
				'\n'
			}, StringSplitOptions.RemoveEmptyEntries);

                if (isRequest)
                {
                    foreach (var item in headersSplit)
                    {
                        SplitHeaderNameAndValue(oSessionHandler.RequestLines, item);
                    }
                }
                else
                {
                    foreach (var item in headersSplit)
                    {
                        SplitHeaderNameAndValue(oSessionHandler.ResponseLines, item);
                    }
                }
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
            }
        }

        private static void SplitHeaderNameAndValue(IDictionary<string, string> inputDic, string item)
        {
            try
            {
                string[] tempHeader = item.Split(ConstantVariables.COLON_SPACE_SPLIT, StringSplitOptions.None);
                if (!inputDic.ContainsKey(tempHeader[0].ToUpper()))
                {
                    inputDic.Add(tempHeader[0].ToUpper(), tempHeader[1]);
                }
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
            }
        }
    }
}
