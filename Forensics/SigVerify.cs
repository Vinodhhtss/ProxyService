using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Forensics
{
    public class CryptInfo
    {
        public string Subject { get; set; }
        public string CA { get; set; }
        public string Signature { get; set; }
        public bool IsSigned { get; set; }
        public bool IsVerified { get; set; }
        public string MD5 { get; set; }
    }

    public class SigVerify
    {
        ////////
        /// This is a simple verification. We don't validate the trust chain.
        ////////

        public static CryptInfo CheckSignatureForFile(string path)
        {
            CryptInfo ci = new CryptInfo();
            if (string.IsNullOrEmpty(path))
            {
                return ci;
            }
            bool isFileExist = false;
            IntPtr wow64Value = IntPtr.Zero;
            if (File.Exists(path))
            {
                isFileExist = true;
            }
            else
            {
                if (path.ToLower().StartsWith(@"c:\windows\system32"))
                {
                    Util.Wow64DisableWow64FsRedirection(ref wow64Value);
                    if (File.Exists(path))
                    {
                        isFileExist = true;
                    }
                }
            }

            if (isFileExist)
            {
                X509Certificate2 localcert = null;

                try
                {
                    X509Certificate localsigner = X509Certificate.CreateFromCertFile(path);
                    localcert = new X509Certificate2(localsigner);
                    ci.IsSigned = true;
                }
                catch (Exception)
                {
                    return ci;
                }

                try
                {
                    var certChain = new X509Chain();

                    certChain.ChainPolicy.RevocationFlag = X509RevocationFlag.EndCertificateOnly;
                    certChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                    certChain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 0, 00);
                    certChain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;

                    if (certChain.Build(localcert))
                    {
                        ci.IsVerified = true;
                    }

                    ci.Subject = localcert.SubjectName.Name;
                    ci.CA = localcert.IssuerName.Name;
                    int tempIndex = ci.Subject.IndexOf("OU");
                    if (tempIndex > 0)
                    {
                        ci.Signature = ci.Subject.Substring(0, tempIndex - 2);
                        ci.Signature = ci.Signature.Replace("\"", string.Empty);
                    }
                }
                catch (Exception)
                {
                    ci.IsVerified = false;
                }
            }

            if (wow64Value != IntPtr.Zero)
            {
                Util.Wow64RevertWow64FsRedirection(wow64Value);
            }
            return ci;
        }
    }
}
