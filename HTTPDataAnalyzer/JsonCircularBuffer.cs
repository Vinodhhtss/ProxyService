using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace HTTPDataAnalyzer
{
    public static class JSONCircularBuffer
    {
        //public static readonly object lockCurrentBuffer = new object();
        //public const int CIRCULAR_BUFFER_SIZE = 200;

        public static void ExportToHAR(Queue<SessionHandler> inputBuffer, string harFilePath, int countToExport)
        {
            if (harFilePath == string.Empty)
            {
                harFilePath = System.IO.Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                                   ConstantVariables.GetAppDataFolder());
            }
            harFilePath = System.IO.Path.Combine(harFilePath, ConstantVariables.CIRCULAR_BUFFER_HAR_FILE_NAME);
            countToExport = ValidExportCount(countToExport);
            int startIndex = TakenRecentPackets(countToExport);
            ExportToHAR(inputBuffer, harFilePath, startIndex, countToExport);
        }

        public static int ValidExportCount(int countToExport)
        {
            return countToExport;
            //if (countToExport <= DBAnalyser.CurrentBuffer.Count)
            //{
            //    return countToExport;
            //}
            //else
            //{
            //    return DBAnalyser.CurrentBuffer.Count;
            //}
        }

        public static int TakenRecentPackets(int countToExport)
        {
            //if (DBAnalyser.CurrentBuffer.Count <= countToExport)
            //{
            //    return 0;
            //}
            //else
            //{
            //    return DBAnalyser.CurrentBuffer.Count - countToExport;
            //}
            return 0;
        }

        public static void ExportToHAR(Queue<SessionHandler> inputBuffer, string harFilePath, int startIndex, int countToExport)
        {
            if (countToExport <= 0)
            {
                countToExport = inputBuffer.Count;
            }
            Automatonic.HttpArchive.Document d = HARDocumentBuilder(inputBuffer, countToExport);

            try
            {
                using (FileStream fs = File.Open(harFilePath, FileMode.Create))
                using (StreamWriter sw = new StreamWriter(fs))
                using (JsonWriter jw = new JsonTextWriter(sw))
                {
                    jw.Formatting = Newtonsoft.Json.Formatting.Indented;

                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(jw, d);
                }
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
                throw;
            }
        }

        public static byte[] ExportToHAR(Queue<SessionHandler> inputBuffer)
        {
            byte[] outputBuffer;
            Automatonic.HttpArchive.Document d = HARDocumentBuilder(inputBuffer, inputBuffer.Count);

            try
            {
                MemoryStream fs = new MemoryStream();
                using (StreamWriter sw = new StreamWriter(fs))
                using (JsonWriter jw = new JsonTextWriter(sw))
                {
                    jw.Formatting = Newtonsoft.Json.Formatting.Indented;

                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(jw, d);

                }
                outputBuffer = fs.ToArray();
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
                throw;
            }

            return outputBuffer;
        }

        private static byte[] ObjectToByteArray(Object obj)
        {
            if (obj == null)
            {
                return null;
            }

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        private static Automatonic.HttpArchive.Document HARDocumentBuilder(Queue<SessionHandler> inputBuffer, int countToExport)
        {
            Automatonic.HttpArchive.Document d = new Automatonic.HttpArchive.Document();
            Automatonic.HttpArchive.Log l = new Automatonic.HttpArchive.Log();
            l.Version = "1.0";
            l.Comment = "Test";

            l.Entries = new List<Automatonic.HttpArchive.Entry>();
            l.Pages = new List<Automatonic.HttpArchive.Page>();
            int count = 0;


            for (int i = 0; i < countToExport; i++)
            {
                var oSessionHndlr = inputBuffer.ElementAt(i);

                Automatonic.HttpArchive.Page page = new Automatonic.HttpArchive.Page();
                page.Id = "page_" + count.ToString();
                page.Comment = string.Empty;
                page.StartedDateTime = oSessionHndlr.RequestStarted.ToString(ConstantVariables.DATETIMEFORMAT); ;
                page.PageTimings = new Automatonic.HttpArchive.PageTimings();
                page.PageTimings.OnContentLoad = -1;
                page.PageTimings.OnLoad = -1;
                page.PageTimings.Comment = string.Empty;
                page.Title = oSessionHndlr.HostName;
                Automatonic.HttpArchive.Entry entry = new Automatonic.HttpArchive.Entry();
                entry.IPAddress = Util.FindIpAdrress();
                entry.ProcessID = oSessionHndlr.ClientID;
                entry.ProcessName = oSessionHndlr.ClientName;
                entry.FirstHeaderLine = oSessionHndlr.FirstHeaderLine;
                entry.PageRef = page.Id;
                count++;
                entry.Request = new Automatonic.HttpArchive.Request();
                entry.Response = new Automatonic.HttpArchive.Response();
                entry.Response.Content = new Automatonic.HttpArchive.Content();
                entry.Request.BodySize = oSessionHndlr.RequestLength;
                entry.Request.HeadersSize = (int)oSessionHndlr.ResponseHeadersSize;
                entry.StartedDateTime = oSessionHndlr.RequestStarted.ToString(ConstantVariables.DATETIMEFORMAT);

                entry.Timings = new Automatonic.HttpArchive.Timings();
                entry.Timings.Ssl = 0;

                entry.Timings.Blocked = 0;
                entry.Timings.Comment = string.Empty;
                entry.Timings.Connect = 1;
                entry.Timings.Wait = 1;
                entry.Time = 0;
                entry.Connection = oSessionHndlr.Port.ToString();
                entry.Comment = string.Empty;
                entry.ServerIPAddress = oSessionHndlr.IPAddress;

                entry.Request.Method = oSessionHndlr.Method;
                if (oSessionHndlr.RequestURL != null)
                {
                    entry.Request.Url = oSessionHndlr.RequestURL;

                    string[] tempQueryParams = oSessionHndlr.RequestURL.Split(ConstantVariables.QUESTION_SPLIT, 2, StringSplitOptions.None);
                    if (tempQueryParams.Count() >= 2)
                    {
                        try
                        {
                            entry.Request.QueryString = new List<Automatonic.HttpArchive.NamedValue>();
                            string[] queryParams = tempQueryParams[1].Split(ConstantVariables.AMP_SPLIT, StringSplitOptions.None);
                            for (int j = 0; j < queryParams.Count(); j++)
                            {
                                var item = queryParams[j];

                                string[] nameAndValue = item.Split(ConstantVariables.EQUAL_SPLIT);
                                Automatonic.HttpArchive.NamedValue named = new Automatonic.HttpArchive.NamedValue();
                                if (nameAndValue.Count() == 2)
                                {
                                    named.Name = nameAndValue[0];
                                    named.Value = nameAndValue[1];
                                }
                                else
                                {
                                    named.Name = nameAndValue[0];
                                    named.Value = string.Empty;
                                }
                                named.Comment = string.Empty;
                                entry.Request.QueryString.Add(named);
                            }

                        }
                        catch (Exception ex)
                        {
                            //AnalyzerManager.Logger.Error(ex);
                        }
                    }
                }
                else
                {
                    entry.Request.Url = string.Empty;
                }
                entry.Request.HttpVersion = oSessionHndlr.RequestHttpVersion;

                CreateHeaderAndCookie(entry, oSessionHndlr.RequestLines, true);
                CreateHeaderAndCookie(entry, oSessionHndlr.ResponseLines, false);
                entry.Response.BodySize = (int)oSessionHndlr.ResponseBodySize;
                entry.Response.HttpVersion = "HTTP/1.0";
                entry.Response.Status = oSessionHndlr.StatusCode;
                entry.Response.StatusText = oSessionHndlr.StatusDescription ?? string.Empty;
                entry.Response.RedirectUrl = oSessionHndlr.RedirectUrl;
                entry.Response.Comment = string.Empty;

                entry.Response.Content = new Automatonic.HttpArchive.Content();
                entry.Response.Content.Comment = string.Empty;
                entry.Response.Content.Encoding = oSessionHndlr.ContentEncoding ?? string.Empty;
                entry.Response.Content.MimeType = oSessionHndlr.ContentMimeType ?? string.Empty;
                entry.Response.Content.Text = string.Empty;// oSessionHndlr.ResponseString;
                if (oSessionHndlr.ResponseRawData != null && oSessionHndlr.ResponseHeadersRawData != null)
                {
                    entry.Response.Content.Size = oSessionHndlr.ResponseRawData.Length + oSessionHndlr.ResponseHeadersRawData.Length;
                }
                else if (oSessionHndlr.ResponseHeadersRawData != null)
                {
                    entry.Response.Content.Size = oSessionHndlr.ResponseHeadersRawData.Length;
                }
                entry.Response.Content.Compression = 0;

                l.Pages.Add(page);
                l.Entries.Add(entry);
            }

            l.Browser = new Automatonic.HttpArchive.Software();
            l.Browser.Name = "ProxyService";
            l.Browser.Version = "1.0";
            l.Browser.Comment = string.Empty;

            l.Creator = new Automatonic.HttpArchive.Software();
            l.Creator.Name = ConstantVariables.HAR_CREATOR;
            l.Creator.Version = ConstantVariables.HAR_VERSION;
            l.Creator.Comment = string.Empty;
            d.Log = l;
            return d;
        }

        public static void CreateHeaderAndCookie(Automatonic.HttpArchive.Entry entry, Dictionary<string, string> headers, bool isRequest)
        {
            try
            {
                List<Automatonic.HttpArchive.NamedValue> tempHeader = new List<Automatonic.HttpArchive.NamedValue>();
                List<Automatonic.HttpArchive.Cookie> tempCookie = new List<Automatonic.HttpArchive.Cookie>();
                foreach (var item in headers)
                {
                    Automatonic.HttpArchive.NamedValue named = new Automatonic.HttpArchive.NamedValue();
                    named.Name = item.Key;
                    named.Value = item.Value;
                    named.Comment = string.Empty;
                    tempHeader.Add(named);

                }
                if (headers.ContainsKey(ConstantVariables.COOKIE))
                {
                    string[] cookies = headers[ConstantVariables.COOKIE].Split(ConstantVariables.SEMI_COLON_SPACE_SPLIT, StringSplitOptions.None);
                    foreach (string cookie in cookies)
                    {
                        string[] nameValue = cookie.Split(ConstantVariables.EQUAL_SPLIT, 2, StringSplitOptions.None);
                        Automatonic.HttpArchive.Cookie named = new Automatonic.HttpArchive.Cookie();
                        if (nameValue.Count() > 1)
                        {
                            named.Name = nameValue[0];
                            named.Value = nameValue[1];
                        }
                        else
                        {
                            named.Name = nameValue[0];
                            named.Value = string.Empty;
                        }
                        named.Comment = string.Empty;
                        tempCookie.Add(named);
                    }
                }
                if (isRequest)
                {
                    entry.Request.Headers = tempHeader;
                    entry.Request.Cookies = tempCookie;
                }
                else
                {
                    entry.Response.Headers = tempHeader;
                    entry.Response.Cookies = tempCookie;
                }
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
            }
        }
    }
}
