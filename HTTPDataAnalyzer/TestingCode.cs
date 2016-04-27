using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace HTTPDataAnalyzer
{
    public class TestingCode
    {
        private static string m_FileLocation = "E:\\CurrentRequestDetails\\";

        public static string XMLFilePath(string filePath)
        {
            if (filePath != null)
            {
                m_FileLocation = filePath.TrimEnd(ConstantVariables.TEXT_EXTENSION.ToCharArray());
            }

            if (!Directory.Exists(m_FileLocation))
            {
                Directory.CreateDirectory(m_FileLocation);
            }
            string fileName = Path.Combine(m_FileLocation, GetDateTime() + ConstantVariables.XML_EXTENSION);
            return fileName;

        }

        public static string GetDateTime()
        {
            DateTime now = DateTime.Now;
            return now.ToString(ConstantVariables.DATE_FORMAT);
        }

        public static void ExportToXML(string xmlFilePath, SessionHandler currentSession)
        {
            List<SessionHandler> lstSesHandler = new List<SessionHandler>();
            lstSesHandler.Add(currentSession);
            ExportToXML(xmlFilePath, lstSesHandler);
        }

        public static void ExportToXML(string xmlFilePath, List<SessionHandler> lstSessions)
        {
            try
            {
                if (xmlFilePath != string.Empty)
                {

                    var elementsGroupByLevel = from element in lstSessions
                                               group element by element.ClientID into newgroup
                                               select newgroup;


                    XElement xmlFileForInterDetails = new XElement(ConstantVariables.PACKETDETAILS, from a in elementsGroupByLevel
                                                                                                    select
                                                                                                                    new XElement(ConstantVariables.CLIENT,
                                                                                                    from b in a
                                                                                                    select new XElement(ConstantVariables.PACKETDETAIL,
                                                                                                     new XElement(ConstantVariables.CLIENTID, a.Key),
                                                                                                     new XElement(ConstantVariables.THREADINDEX, b.ThreadIndex),
                                                                                                      new XElement(ConstantVariables.FIRSTHEADERLINE, b.FirstHeaderLine),
                                                                                                     new XElement(ConstantVariables.REQUEST,
                                                                                                     new XElement(ConstantVariables.HEADERS, from str in b.RequestLines
                                                                                                                                             select
                                                                                                                                               new XElement(CleanInvalidXmlChars(str.Key, true), CleanInvalidXmlChars(str.Value, false))),
                                                                                                                                                  new XElement(ConstantVariables.REQUESTBODY, CleanInvalidXmlChars(MessageDecoderRequest(b.RequestRawData), false))

                                                                                               ),
                                                                                                                  new XElement(ConstantVariables.RESPONSE,
                                                                                                                                                new XElement(ConstantVariables.HEADERS, from str in b.ResponseLines
                                                                                                                                                                                        select
                                                                                                                                                                                                new XElement(CleanInvalidXmlChars(str.Key, true), CleanInvalidXmlChars(str.Value, false)))
                                                                                                                        ))));

                    xmlFileForInterDetails.Save(xmlFilePath, System.Xml.Linq.SaveOptions.DisableFormatting);
                }
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
            }
        }

        public static string CleanInvalidXmlChars(string inString, bool isHeader)
        {
            if (inString == null) return string.Empty;

            string tempDecode = System.Web.HttpUtility.UrlDecode(inString);
            string tempHtml = System.Web.HttpUtility.HtmlDecode(tempDecode);
            string returnValue = string.Empty;
            StringBuilder newString = new StringBuilder();
            char ch;

            for (int i = 0; i < tempHtml.Length; i++)
            {
                ch = tempHtml[i];
                // remove any characters outside the valid UTF-8 range as well as all control characters
                // except tabs and new lines
                //if ((ch < 0x00FD && ch > 0x001F) || ch == '\t' || ch == '\n' || ch == '\r')
                //if using .NET version prior to 4, use above logic
                if (XmlConvert.IsXmlChar(ch)) //this method is new in .NET 4
                {
                    newString.Append(ch);
                }
            }

            if (isHeader)
            {
                string[] temp = newString.ToString().Split(ConstantVariables.XML_NAME_INVALID_CHAR, StringSplitOptions.RemoveEmptyEntries);
                returnValue = String.Join("\n", temp);
                return returnValue;
            }
            else
            {
                returnValue = newString.ToString();
            }

            return returnValue;
        }

        public static string MessageDecoderRequest(byte[] tempBytes)
        {
            string temp = string.Empty;
            try
            {
                temp = Encoding.UTF8.GetString(tempBytes);

            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
            }

            return temp;
        }
    }
}
