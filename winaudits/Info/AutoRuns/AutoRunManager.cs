using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace winaudits
{
    public class AutoRunManager
    {
        public static List<Autorunpoints> StartAudit()
        {
            List<winaudits.Autorunpoints> regRunPoints = winaudits.RegistryRun.StartAudit();
            List<winaudits.Autorunpoints> fileRunPoints = winaudits.XLSStart.StartAudit();
            List<winaudits.Autorunpoints> bhoRun = winaudits.RegistryBHO.StartAudit();
            List<winaudits.Autorunpoints> appInits = winaudits.AppInitDLL.StartAudit();
            regRunPoints.AddRange(fileRunPoints);
            regRunPoints.AddRange(bhoRun);
            regRunPoints.AddRange(appInits);

            foreach (var item in regRunPoints)
            {
                Forensics.CryptInfo ci;

                ci = Forensics.SigVerify.CheckSignatureForFile(item.FilePath);
                ci.MD5 = Forensics.ProxyMD5.ComputeFileMD5(item.FilePath);

                item.FileMD5 = ci.MD5;
                item.IsSigned = ci.IsSigned;
                item.IsVerified = ci.IsVerified;
                item.CertSubject = ci.Subject;
                item.CA = ci.CA;
                item.SignatureString = ci.Signature;
                ExtractFileInfo(item);
            }

            return regRunPoints;
        }

        private static void ExtractFileInfo(Autorunpoints runPoint)
        {
            bool isFileExist = false;

            if (string.IsNullOrEmpty(runPoint.FilePath))
            {
                return;
            }

            IntPtr wow64Value = IntPtr.Zero;
            if (File.Exists(runPoint.FilePath))
            {
                isFileExist = true;
            }
            else
            {
                if (runPoint.FilePath.ToLower().StartsWith(@"c:\windows\system32"))
                {
                    Forensics.Util.Wow64DisableWow64FsRedirection(ref wow64Value);
                    if (File.Exists(runPoint.FilePath))
                    {
                        isFileExist = true;
                    }
                }
            }

            if (isFileExist)
            {
                try
                {

                    FileInfo fi = new FileInfo(runPoint.FilePath);

                    runPoint.FileCreated = fi.CreationTimeUtc.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    runPoint.FileModified = fi.LastWriteTimeUtc.ToString("yyyy-MM-dd HH:mm:ss.fff");

                    FileSecurity fileSecurity = File.GetAccessControl(runPoint.FilePath);
                    IdentityReference sid = fileSecurity.GetOwner(typeof(SecurityIdentifier));
                    NTAccount ntAccount = sid.Translate(typeof(NTAccount)) as NTAccount;
                    runPoint.FileOwner = ntAccount.Value;
                }
                catch (Exception)
                {

                }
            }
            if (wow64Value != IntPtr.Zero)
            {
                Forensics.Util.Wow64RevertWow64FsRedirection(wow64Value);
            }
        }
    }
}
