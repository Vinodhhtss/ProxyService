using System;
using System.IO;
using System.Text;

namespace HTTPDataAnalyzer
{
    public class MIMETypeIdentifier
    {
        public static void GetStreamMIMEType(SessionHandler oSessionHndlr, Byte[] buffer, bool isRequest)
        {
            if (buffer.Length > 0)
            {
                if (isRequest)
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
                                tempBuffer = Decompressor.DecompressGzip(new MemoryStream(tempBuffer), Encoding.Default);
                                break;
                        }                        
                    }
                    oSessionHndlr.DownloadStream.MIME_DLL = MIMEIdentifier.FileMIME.GetMimeFromBytes(buffer);
                    oSessionHndlr.DownloadStream.MIME_Signature = MIMEIdentifier.FileMIMESignature.GetMimeType(buffer);
                }
            }
        }
    }
}
