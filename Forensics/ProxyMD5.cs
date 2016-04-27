using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace Forensics
{
    public class ProxyMD5
    {
        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }

        public string ComputeMD5(String inputval)
        {
            string retval = null;
            using (MD5 md5hash = MD5.Create())
            {
                retval = GetMd5Hash(md5hash, inputval);
            }
            return retval;
        }

        public bool ComputeFileMD5(out String md5val, String filename)
        {
            md5val = String.Empty;

            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    byte[] data = md5.ComputeHash(stream);
                    StringBuilder sBuilder = new StringBuilder();

                    // Loop through each byte of the hashed data  
                    // and format each one as a hexadecimal string. 
                    for (int i = 0; i < data.Length; i++)
                    {
                        sBuilder.Append(data[i].ToString("x2"));
                    }

                    md5val = sBuilder.ToString();
                    // Return the hexadecimal string. 
                    return true;
                }
            }
            return false;
        }


        public static String ComputeFileMD5(String filename)
        {
            bool isFileExist = false;
            String md5val = String.Empty;

            if (string.IsNullOrEmpty(filename))
            {
                return md5val;
            }

            IntPtr wow64Value = IntPtr.Zero;
            if (File.Exists(filename))
            {
                isFileExist = true;
            }
            else
            {
                if (filename.ToLower().StartsWith(@"c:\windows\system32"))
                {
                    Util.Wow64DisableWow64FsRedirection(ref wow64Value);
                    if (File.Exists(filename))
                    {
                        isFileExist = true;
                    }
                }
            }

            if (isFileExist)
            {
                try
                {
                    using (var md5 = MD5.Create())
                    {
                        using (var stream = File.OpenRead(filename))
                        {
                            byte[] data = md5.ComputeHash(stream);
                            StringBuilder sBuilder = new StringBuilder();

                            // Loop through each byte of the hashed data  
                            // and format each one as a hexadecimal string. 
                            for (int i = 0; i < data.Length; i++)
                            {
                                sBuilder.Append(data[i].ToString("x2"));
                            }

                            md5val = sBuilder.ToString();
                            // Return the hexadecimal string. 
                            return md5val;
                        }
                    }
                }
                catch (Exception)
                {

                }
            }

            if (wow64Value != IntPtr.Zero)
            {
                Util.Wow64RevertWow64FsRedirection(wow64Value);
            }

            return md5val;
        }
    }

    public class Util
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Wow64DisableWow64FsRedirection(ref IntPtr ptr);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool Wow64RevertWow64FsRedirection(IntPtr ptr);
    }
}
