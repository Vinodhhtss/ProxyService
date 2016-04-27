namespace HTTPDataAnalyzer
{
    using System;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Linq;
    using System.Collections.Generic;
    using HttpMultipartParser;
    using System.IO;

    public class ProxyServiceAPI
    {
        private SessionHandler m_SessHandler;

        public ProxyServiceAPI(SessionHandler oSessionHndlr)
        {
            m_SessHandler = oSessionHndlr;
        }

        public string GetURL()
        {
            return m_SessHandler.HostName;
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
            //m_SessHandler.LuaLogger.WriteLogInfo("Enter into header Existing Check");
            if (m_SessHandler.RequestLines.ContainsKey(headerName.ToUpper()))
            {
                if (m_SessHandler.RequestLines[headerName.ToUpper()] == headerValue)
                {
                    return true;
                }
            }
            //m_SessHandler.LuaLogger.WriteLogInfo("Exit from header Existing Check");
            return false;
        }

        public string GetRequestHeaderValue(string headerName)
        {
            //m_SessHandler.LuaLogger.WriteLogInfo("Getting Header Value");

            if (m_SessHandler.RequestLines.ContainsKey(headerName.ToUpper()))
            {
                string strHeaderName = m_SessHandler.RequestLines[headerName.ToUpper()];
                return strHeaderName;
            }
            return string.Empty;
        }

        public string GetResponseHeaderValue(string headerName)
        {
            //m_SessHandler.LuaLogger.WriteLogInfo("Getting Header Value");
            if (m_SessHandler.ResponseLines.ContainsKey(headerName.ToUpper()))
            {
                string strHeaderName = m_SessHandler.ResponseLines[headerName.ToUpper()];
                return strHeaderName;
            }
            return string.Empty;
        }

        public string GetRequestBody()
        {
            //m_SessHandler.LuaLogger.WriteLogInfo("Getting RequestBody Value");
            if (m_SessHandler.RequestHeadersRawData != null)
            {
                string temp = MessagesDecoder.MessageDecoderRequest(m_SessHandler);

                return System.Web.HttpUtility.UrlDecode(temp);
            }
            return string.Empty;
        }

        public byte[] GetRawRequestBody()
        {
            //m_SessHandler.LuaLogger.WriteLogInfo("Getting RequestBody Value");

            return m_SessHandler.RequestRawData;
        }

        public bool HostContains(string toMatchString)
        {
            if (m_SessHandler.HostName.Contains(toMatchString))
            {
                return true;
            }
            else
            {
                return false;
            }
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
                    //AnalyzerManager.Logger.Error(ex);
                    ret = true;
                }
            }
            return ret;
        }

        public bool StartWith(string original, string startString)
        {
            if (original.StartsWith(startString, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        public bool ContainString(string original, string subString)
        {
            if (original.Contains(subString))
            {
                return true;
            }
            return false;
        }

        public int IndexOfGiven(string original, string startString)
        {
            //m_SessHandler.LuaLogger.WriteLogInfo("Finding Index of string");
            return original.TrimStart('&').IndexOf(startString);

        }

        public string ReplaceString(string original, string replaceString, string replacingstring)
        {

            //m_SessHandler.LuaLogger.WriteLogInfo("Finding Index of string");
            original = original.Replace(replaceString, replacingstring);
            return original;
        }

        public string GetPartOfString(string original, int start, int end)
        {
            try
            {
                return original.Substring(start, end);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message + " " + ex.StackTrace);
            }
            return string.Empty;
        }


        public string GetClientName()
        {
            return m_SessHandler.ClientName;
        }

        public int GetProcessId()
        {
            return m_SessHandler.ClientID;
        }

        public string GetDateTime()
        {
            DateTime now = DateTime.Now;
            return now.ToString("ddMMyyyyHHmmss");
        }

        public void SetFileName(string fileName)
        {
            m_SessHandler.FileName = fileName;
        }

        public void WriteToXML(string path, string msg, bool isUPload)
        {
            msg = msg.Replace("%40", "@");
            TestingCode.ExportToXML(TestingCode.XMLFilePath(path), m_SessHandler);
            if (!FilePathIsInvalid(path, ConstantVariables.TEXT_EXTENSION))
            {
                System.IO.File.AppendAllText(path, msg + Environment.NewLine);
            }
        }

        public void AnalyseMultiFormData(byte[] originArray, string subString)
        {
            //TestingCode.ExportToXML(TestingCode.XMLFilePath("E:\\linkedinlogin.txt"), m_SessHandler);
            //byte[] subStringBytes = System.Text.Encoding.ASCII.GetBytes(subString);
            //List<int> index = Util.SearchBytePattern(subStringBytes, originArray);
            //if (index.Count > 0)
            //{
            //    return index.LastOrDefault() - 1 + subString.Length;
            //}
            //return 0;
            //List<int> lastIndex =    SearchBytePattern( b2, tempBuffer);
            //byte[] b3 = new byte[tempBuffer.Length - lastIndex.LastOrDefault()- 1];
            //Array.Copy(tempBuffer, lastIndex.LastOrDefault() + 4, b3, 0, b3.Length - index - 1);
        }

        public void AnalyseMultiFormData()
        {
            var parser = new MultipartFormDataParser(new MemoryStream(m_SessHandler.RequestRawData));
            // From this point the data is parsed, we can retrieve the
            // form data from the GetParameterValues method
            //  var checkboxResponses = parser.GetParameterValues()
            foreach (var file in parser.Files)
            {
                m_SessHandler.Files.Add(ReadFully(file.Data));
                break;
                // Do stuff with the data.
            }
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public void WriteToFile(string fileName, int fileStartIndex, bool isUPload)
        {
           
            //var parser = new MultipartFormDataParser(new MemoryStream(m_SessHandler.RequestRawData));
            Stream data = null;
            //// From this point the data is parsed, we can retrieve the
            //// form data from the GetParameterValues method
            ////  var checkboxResponses = parser.GetParameterValues()
            //foreach (var file in parser.Files)
            //{
            //     data = file.Data;

            //    // Do stuff with the data.
            //}
            //msg = msg.Replace("%40", "@");
            if (fileName != string.Empty)
            {
                m_SessHandler.FileName = fileName;
            }

            if (isUPload)
            {
                AnalyseMultiFormData();
                if (fileStartIndex > 0)
                {
                    m_SessHandler.RequestRawData = ReadFully(data);//m_SessHandler.RequestRawData.Skip(fileStartIndex).ToArray();
                }

                if (m_SessHandler.RequestLines.ContainsKey("FILENAME"))
                {
                    m_SessHandler.FileName = m_SessHandler.RequestLines["FILENAME"];
                }

                if (m_SessHandler.Files.Count > 0)
                {
                    FileHandler.WriteToFile(m_SessHandler, m_SessHandler.Files[0], true);
                }

                else
                {
                    FileHandler.WriteToFile(m_SessHandler, m_SessHandler.RequestRawData, true);
                }
            }
            else
            {
                if (fileStartIndex > 0)
                {
                    m_SessHandler.RequestRawData = ReadFully(data);// m_SessHandler.ResponseRawData.Skip(fileStartIndex).ToArray();
                }
                FileHandler.WriteToFile(m_SessHandler, m_SessHandler.ResponseRawData, false);
            }
        }
    }
}
