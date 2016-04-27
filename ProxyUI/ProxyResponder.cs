using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ProxyUI
{
    public class ProxyResponder
    {
        public string Method { get; set; }
        public string httpVersion { get; set; }
        public string RequestURL { get; set; }
        public string localPath { get; set; }

        public string HostName { get; set; }
        // public CustomBinaryReader ClientStreamReader { get; set; }
        public int RequestLength { get; set; }
        public Stream ClientStream { get; set; }
        public Dictionary<string, string> RequestLines { get; set; }

        public string rootWeb { get; set; }

        public WebMap wbmap { get; set; }


        public ProxyResponder()
        {
            RequestLines = new Dictionary<string, string>();
        }

        public void SplitAndSetUrl(string remurl)
        {
            Uri localuri = new Uri(remurl);

            localPath = localuri.AbsolutePath;
        }

        public void PopulateRequestHeaders(ref List<string> reqLines)
        {
            for (int i = 1; i < reqLines.Count; i++)
            {
                String httpCmd = reqLines[i];

                String[] header = reqLines[i].Split(ConstVars.COLON_SPACE_SPLIT, 2, StringSplitOptions.None);

                try
                {
                    if (!String.IsNullOrEmpty(header[0].Trim()))
                    {
                        if (header.Length > 1)
                        {
                            if (!this.RequestLines.ContainsKey(header[0]))
                            {
                                this.RequestLines.Add(header[0].Trim(), header[1].Trim());
                            }
                            else
                            {
                                this.RequestLines[header[0]] += header[1];
                            }
                        }
                        else
                        {
                            this.RequestLines.Add(header[0].Trim(), string.Empty);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message + " " + ex.StackTrace);
                }

                header = null;
            }
        }

        private void SendHeader(string mimeHeader, int totBytes, string statusCode)
        {
            String sBuffer = "";

            // if Mime type is not provided set default to text/html
            if (mimeHeader.Length == 0)
            {
                mimeHeader = "text/html";  // Default Mime Type is text/html
            }

            sBuffer = sBuffer + httpVersion + statusCode + "\r\n";
            sBuffer = sBuffer + "Server: spellsec-b\r\n";
            sBuffer = sBuffer + "Content-Type: " + mimeHeader + "\r\n";
            sBuffer = sBuffer + "Accept-Ranges: bytes\r\n";
            sBuffer = sBuffer + "Content-Length: " + totBytes + "\r\n\r\n";

            Byte[] bSendData = Encoding.ASCII.GetBytes(sBuffer);

            ClientStream.Write(bSendData, 0, sBuffer.Length);
        }

        private void SendBody(string body)
        {
            Byte[] bSendData = Encoding.ASCII.GetBytes(body);

            ClientStream.Write(bSendData, 0, body.Length);
        }

        private void SendBodyBinary(byte[] body)
        {
            ClientStream.Write(body, 0, body.Length);
        }

        private void SendBlockPage(String blockpage)
        {
            Byte[] bSendData = Encoding.ASCII.GetBytes(blockpage);
            ClientStream.Write(bSendData, 0, bSendData.Length);
        }

        public void RespondBlockPage()
        {
            String bp = "<html><body>Block Page</body></html>";

            SendHeader("", bp.Length, "404");
            SendBlockPage(bp);
            ClientStream.Flush();
            ClientStream.Close();
        }

        public bool GetBinaryFile(string fpath, out byte[] srep)
        {
            bool error = false;
            byte[] fileContent = { 0 };
            srep = null;

            try
            {
                srep = System.IO.File.ReadAllBytes(fpath);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message + " " + ex.StackTrace);
                error = true;
            }

            if (srep != null && srep.Length > 0 && error == false)
            {
                return true;
            }

            return false;
        }

        public bool GetFileStream(string fpath, out String sResponse)
        {
            bool error = false;
            int iTotBytes = 0;
            sResponse = "";

            try
            {
                FileStream fs = new FileStream(fpath, FileMode.Open, FileAccess.Read, FileShare.Read);

                BinaryReader reader = new BinaryReader(fs);
                byte[] bytes = new byte[fs.Length];

                int read;

                while ((read = reader.Read(bytes, 0, bytes.Length)) != 0)
                {
                    sResponse = sResponse + Encoding.ASCII.GetString(bytes, 0, read);
                    iTotBytes = iTotBytes + read;
                }

                reader.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message + " " + ex.StackTrace);
                error = true;
            }

            if (iTotBytes > 0 && error == false && sResponse != "")
            {
                return true;
            }

            return false;
        }

        public bool mimetypeAscii(String mime)
        {
            bool retval = false;

            if (mime.Equals("text/css") == true)
            {
                retval = true;
            }
            else
                if (mime.Equals("text/html") == true)
                {
                    retval = true;
                }
                else
                    if (mime.Equals("application/javascript") == true)
                    {
                        retval = true;
                    }

            return retval;
        }

        public void RespondToRequest()
        {
            string fpath = "";
            string mimetype = "";

            if (localPath != null)
            {
                fpath = wbmap.GetPath(localPath);
                mimetype = wbmap.GetMime(localPath);
            }

            if (fpath != "" && mimetype != "")
            {
                String retstring = "";

                if (mimetypeAscii(mimetype) == true)
                {
                    if (GetFileStream(fpath, out retstring) == true)
                    {
                        SendHeader(mimetype, retstring.Length, "200");
                        SendBody(retstring);
                        ClientStream.Flush();
                        ClientStream.Close();
                    }
                    else
                    {
                        RespondBlockPage();
                    }
                }
                else
                {
                    byte[] bout = null;

                    if (GetBinaryFile(fpath, out bout) == true)
                    {
                        SendHeader(mimetype, bout.Length, "200");
                        SendBodyBinary(bout);
                        ClientStream.Flush();
                        ClientStream.Close();

                    }
                    else
                    {
                        RespondBlockPage();
                    }
                }
            }
            else
            {
                RespondBlockPage();
            }
        }
    }
}
