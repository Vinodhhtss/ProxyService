using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MIMEIdentifier
{
    public class FileMIME
    {
        private static readonly Dictionary<string, string> MIMETypesDictionary = new Dictionary<string, string>
        {
           {"application/x-msdownload","exe"},
           {"application/pdf", "pdf"},
           {"application/octet-stream", "Unknown"},
           {"video/mpeg", "mp3"}               
        };

        [DllImport(@"urlmon.dll", CharSet = CharSet.Auto)]
        private extern static System.UInt32 FindMimeFromData(
            System.UInt32 pBC,
            [MarshalAs(UnmanagedType.LPStr)] System.String pwzUrl,
            [MarshalAs(UnmanagedType.LPArray)] byte[] pBuffer,
            System.UInt32 cbSize,
            [MarshalAs(UnmanagedType.LPStr)] System.String pwzMimeProposed,
            System.UInt32 dwMimeFlags,
            out System.UInt32 ppwzMimeOut,
            System.UInt32 dwReserverd
        );

        public static string GetMimeFromBytes(byte[] byteArray)
        {

            byte[] buffer = new byte[256];
            using (MemoryStream fs = new MemoryStream(byteArray))
            {
                if (fs.Length >= 256)
                {
                    fs.Read(buffer, 0, 256);
                }
                else
                {
                    fs.Read(buffer, 0, (int)fs.Length);
                }
            }
            try
            {
                System.UInt32 mimetype;
                FindMimeFromData(0, null, buffer, 256, null, 0, out mimetype, 0);
                System.IntPtr mimeTypePtr = new IntPtr(mimetype);
                string mime = Marshal.PtrToStringUni(mimeTypePtr);
                Marshal.FreeCoTaskMem(mimeTypePtr);
                return mime;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public static string GetMimeFromFile(string filename)
        {
            byte[] buffer = new byte[256];
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                if (fs.Length >= 256)
                {
                    fs.Read(buffer, 0, 256);
                }
                else
                {
                    fs.Read(buffer, 0, (int)fs.Length);
                }
            }
            try
            {
                string test = HexStringFromBytes(buffer);
                System.UInt32 mimetype;
                FindMimeFromData(0, null, buffer, 256, null, 0, out mimetype, 0);
                System.IntPtr mimeTypePtr = new IntPtr(mimetype);
                string mime = Marshal.PtrToStringUni(mimeTypePtr);
                Marshal.FreeCoTaskMem(mimeTypePtr);
                string val2;
                if (MIMETypesDictionary.TryGetValue(mime, out val2) == true)
                {
                    return val2;
                }
                return "unknown";
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message + " " + ex.StackTrace);
                return "unknown";
            }
        }

        /// <summary>
        /// Convert an array of bytes to a string of hex digits
        /// </summary>
        /// <param name="bytes">array of bytes</param>
        /// <returns>String of hex digits</returns>
        private static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }
    }
}

