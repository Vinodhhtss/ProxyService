namespace HTTPProxyServer
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public class ProxyServiceAPI
    {
        private SessionHandler m_SessHandler;
        private static object Locker = new object();

        public ProxyServiceAPI(SessionHandler oSessionHndlr)
        {
            m_SessHandler = oSessionHndlr;
        }

        public string GetURL()
        {
            return m_SessHandler.RequestURL;
        }

        public bool IsRequestHeaderExists(string headerName)
        {
            if (m_SessHandler.RequestLines.ContainsKey(headerName.ToUpper()))
            {
                return true;
            }
            return false;
        }

        public bool IsResponseHeaderExists(string headerName)
        {
            if (m_SessHandler.ResponseLines.ContainsKey(headerName.ToUpper()))
            {
                return true;
            }
            return false;
        }

        public bool IsHeaderExistsWithValue(string headerName, string headerValue)
        {
            //////m_SessHandler.LuaLogger.Logger.Info("Enter");
            if (m_SessHandler.RequestLines.ContainsKey(headerName.ToUpper()))
            {
                if (m_SessHandler.RequestLines[headerName.ToUpper()] == headerValue)
                {
                    return true;
                }
            }
            ////m_SessHandler.LuaLogger.Logger.Info("Exit");
            return false;
        }

        public string GetRequestHeaderValue(string headerName)
        {
            ////m_SessHandler.LuaLogger.Logger.Info("Getting Header Value");
            if (m_SessHandler.RequestLines.ContainsKey(headerName.ToUpper()))
            {
                string strHeaderName = m_SessHandler.RequestLines[headerName.ToUpper()];
                return strHeaderName;
            }
            return string.Empty;
        }

        public string GetResponseHeaderValue(string headerName)
        {
            ////m_SessHandler.LuaLogger.Logger.Info("Getting Header Value");
            if (m_SessHandler.ResponseLines.ContainsKey(headerName.ToUpper()))
            {
                string strHeaderName = m_SessHandler.ResponseLines[headerName.ToUpper()];
                return strHeaderName;
            }
            return string.Empty;
        }

        public string GetRequestBody()
        {
            ////m_SessHandler.LuaLogger.Logger.Info("Getting RequestBody Value");
            if (m_SessHandler.RequestRawData != null)
            {
                return MessagesDecoder.DecodeMessage(m_SessHandler, false);
            }
            return string.Empty;
        }

        public bool HostContains(string toMatchString)
        {
            if (m_SessHandler.RequestURL.Contains(toMatchString))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void BlockURL()
        {
            m_SessHandler.BlockURL = true;
        }


        public bool FilePathIsInvalid(string path, string fileExtension)
        {
            bool ret = false;
            if (!string.IsNullOrEmpty(path))
            {
                try
                {
                    string fileName = System.IO.Path.GetFileName(path);
                    string fileDirectory = System.IO.Path.GetDirectoryName(path);
                    if (!fileName.ToLower().EndsWith(fileExtension))
                    {
                        ret = true;
                    }
                }
                catch (Exception ex)
                {
                    ////TCPClientProcessor.Proxylog.Logger.Error(ex);
                    ret = true;
                }
            }
            return ret;
        }

        public bool StartWith(string original, string startString)
        {
            //TestingCode.ExportToXML(TestingCode.XMLFilePath(), m_SessHandler);
            if (original.StartsWith(startString, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        public int IndexOfGiven(string original, string startString)
        {
            //TestingCode.ExportToXML(TestingCode.XMLFilePath(), m_SessHandler);
            ////m_SessHandler.LuaLogger.Logger.Info("Finding Index of string");
            return original.TrimStart('&').IndexOf(startString);

            // return index;
        }

        public string ReplaceString(string original, string replaceString, string replacingstring)
        {
            //TestingCode.ExportToXML(TestingCode.XMLFilePath(), m_SessHandler);

            ////m_SessHandler.LuaLogger.Logger.Info("Finding Index of string");
            original = original.Replace(replaceString, replacingstring);
            return original;

            // return index;
        }

        public string GetPartOfString(string original, int start, int end)
        {
            try
            {
                return original.Substring(start, end);
            }
            catch (Exception ex)
            {
                ////TCPClientProcessor.Proxylog.Logger.Error(ex);
            }
            return string.Empty;
        }


        public string GetClientName()
        {
            //TestingCode.ExportToXML(TestingCode.XMLFilePath(), m_SessHandler);
            return m_SessHandler.ClientName;
        }
        public int GetProcessId()
        {
            //TestingCode.ExportToXML(TestingCode.XMLFilePath(), m_SessHandler);
            return m_SessHandler.ClientID;
        }

        public string GetDateTime()
        {
            // TestingCode.ExportToXML(TestingCode.XMLFilePath(), m_SessHandler);
            DateTime now = DateTime.Now;
            return now.ToString("yyyyMMddHHmmssfff");
        }

        public void WriteToFile(string path, string msg, string fileName, bool isUpload)
        {
            msg = msg.Replace("%40", "@");
            TestingCode.ExportToXML(TestingCode.XMLFilePath(path), m_SessHandler);
            if (!FilePathIsInvalid(path, ConstantVariables.TEXT_EXTENSION))
            {
                // TestingCode.ExportToXML(path.Replace(ConstantVariables.TEXT_EXTENSION, ConstantVariables.XML_EXTENSION), m_SessHandler);

                System.IO.File.AppendAllText(path, msg + Environment.NewLine);
                if (fileName != null)
                {
                    // ExportToXML(fileName, isUpload);
                }
            }
        }


        public void ExportToXML(string fileName, bool isUpload)
        {
            if (fileName == null)
            {
            return;
            }
            lock (Locker)
            {
                TestingCode.ExportToXML(TestingCode.XMLFilePath("E:\\temp\\"), m_SessHandler);
                FileStream tempFilsStream;
                if (fileName != string.Empty)
                {
                    fileName = fileName.Replace("\"", string.Empty);
                    tempFilsStream = new FileStream("E:\\temp\\" + GetDateTime() + fileName, FileMode.Create);
                }
                else
                {
                    tempFilsStream = new FileStream("E:\\temp\\" + GetDateTime(), FileMode.Create);
                }
                if (isUpload)
                {
                    if (m_SessHandler.RequestRawData != null)
                    {
                        tempFilsStream.Write(m_SessHandler.RequestRawData, 0, m_SessHandler.RequestRawData.Length);
                    }
                }
                else
                {
                    if (m_SessHandler.ResponseRawData != null)
                    {
                        tempFilsStream.Write(m_SessHandler.ResponseRawData, 0, m_SessHandler.ResponseRawData.Length);
                    }
                }
                tempFilsStream.Close();

            }
        }

        //public  string GetDateTime()
        //{
        //    DateTime now = DateTime.Now;
        //    return now.ToString(ConstantVariables.DATE_FORMAT);
        //}
    }
}
