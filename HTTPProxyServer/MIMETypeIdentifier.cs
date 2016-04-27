using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HTTPProxyServer
{
    class MIMETypeIdentifier
    {
        public static void GetStreamMIMEType(SessionHandler oSessionHndlr, Byte[] buffer, bool isUpload)
        {
            try
            {
                if (buffer != null && buffer.Length > 0)
                {
                    if (isUpload)
                    {
                        oSessionHndlr.UploadStream.MIME_DLL = MIMEIdentifier.FileMIME.GetMimeFromBytes(buffer);
                        oSessionHndlr.UploadStream.MIME_Signature = MIMEIdentifier.FileMIMESignature.GetMimeType(buffer);
                    }
                    else
                    {

                        byte[] tempBuffer = buffer;
                        if (oSessionHndlr.ResponseLines.ContainsKey("CONTENT-ENCODING"))
                        {
                            switch (oSessionHndlr.ResponseLines["CONTENT-ENCODING"])
                            {
                                case "gzip":
                                    tempBuffer = DecompressGzip(new MemoryStream(tempBuffer), Encoding.Default, oSessionHndlr);
                                    break;
                            }
                        }
                        if (tempBuffer != null)
                        {
                            oSessionHndlr.DownloadStream.MIME_DLL = MIMEIdentifier.FileMIME.GetMimeFromBytes(tempBuffer);
                            oSessionHndlr.DownloadStream.MIME_Signature = MIMEIdentifier.FileMIMESignature.GetMimeType(tempBuffer);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ////oSessionHndlr.Logger.Logger.Error(ex);
            }
        }

        private static byte[] DecompressGzip(Stream input, Encoding e, SessionHandler oSessionHndlr)
        {
            byte[] tempOutput = null;
            try
            {
                using (System.IO.Compression.GZipStream decompressor = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress))
                {
                    int read = 0;
                    var buffer = new byte[375];

                    using (MemoryStream output = new MemoryStream())
                    {
                        while ((read = decompressor.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            output.Write(buffer, 0, read);
                        }
                        tempOutput = output.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                ////oSessionHndlr.Logger.Logger.Error(ex);
            }
            return tempOutput;
        }
    }
}
