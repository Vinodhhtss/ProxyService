using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MIMEIdentifier
{
    public class FileMIMESignature
    {
        private static readonly string MP3Signature = "49-44-33";
        private static readonly string MP4Signature = "00-00-00-18-66-74-79-70";
        private static readonly string PPTSignature = " 00-6E-1E-F0";
        private static readonly string PDFSignature = "25-50-44-46";
        private static readonly string ZIPSignature = "50-4B-03-04";
        private static readonly string DOCSignature = "D0-CF-11-E0-A1-B1-1A-E1";
        private static readonly string JPGSignature = "FF-D8-FF-E0";
        private static readonly string EXESignature = "4D-5A";
        private static readonly string BMPSignature = "42-4D";
        private static readonly string PNGSignature = "89-50-4E-47-0D-0A-1A-0A";
        private static readonly string RARSignature = "52-61-72-21-1A-07";
        private static readonly string PSDSignature = "38-42-50-53";
        private static readonly string ICOSignature = "00-00-01-00";

        private static readonly string SWFSignature1 = "5A-57-53"; //ZWS
        private static readonly string SWFSignature2 = "46-57-53"; //FWS
        private static readonly string SWFSignature3 = "43-57-53"; //CWS

        public static string GetMimeType(byte[] file)
        {  

            string mime = "application/octet-stream";

            try
            {
                if (GetHexStringFrom(file.Take(4).ToArray()) == PPTSignature)
                {
                    mime = "audio/mpeg";
                }
                else if (GetHexStringFrom(file.Take(4).ToArray()) == PSDSignature)
                {
                    mime = "application/x-photoshop";
                }
                else if (GetHexStringFrom(file.Take(4).ToArray()) == ICOSignature)
                {
                    mime = "image/x-icon";
                }
                else if (GetHexStringFrom(file.Take(4).ToArray()) == PDFSignature)
                {
                    mime = "application/pdf";
                }
                else if (GetHexStringFrom(file.Take(3).ToArray()) == MP3Signature)
                {
                    mime = "audio/mpeg";
                }
                else if (GetHexStringFrom(file.Take(8).ToArray()) == MP4Signature)
                {
                    mime = "video/mp4";
                }
                else if (GetHexStringFrom(file.Take(4).ToArray()) == ZIPSignature)
                {
                    mime = "application/x-zip-compressed";
                }
                else if (GetHexStringFrom(file.Take(8).ToArray()) == DOCSignature)
                {
                    mime = "application/msword";
                }
                else if (GetHexStringFrom(file.Take(4).ToArray()) == JPGSignature)
                {
                    mime = "image/jpeg";
                }
                else if (GetHexStringFrom(file.Take(2).ToArray()) == EXESignature)
                {
                    mime = "application/x-msdownload";
                }
                else if (GetHexStringFrom(file.Take(2).ToArray()) == BMPSignature)
                {
                    mime = "image/bmp";
                }
                else if (GetHexStringFrom(file.Take(8).ToArray()) == PNGSignature)
                {
                    mime = "image/x-png";
                }
                else if (GetHexStringFrom(file.Take(6).ToArray()) == RARSignature)
                {
                    mime = "application/x-rar-compressed";
                }
                else
                {
                    string resval = GetHexStringFrom(file.Take(3).ToArray());

                    if (resval == SWFSignature1 || resval == SWFSignature2 || resval == SWFSignature3)
                    {
                        mime = "application/x-shockwave-flash";
                    }
                }
            }
            catch (Exception)
            {

            }
            return mime;
        }

        public static string HexStringFromBytes(IEnumerable<byte> bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }

        private static string GetHexStringFrom(byte[] byteArray)
        {
            return BitConverter.ToString(byteArray);
        }
    }
}