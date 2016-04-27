using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace Forensics
{
    public class ForensicApi
    {
        bool jsint = false;
        UsnJournal.Win32Api.USN_JOURNAL_DATA jns;

      
        public ForensicApi()
        {
           jns = new UsnJournal.Win32Api.USN_JOURNAL_DATA();
        }

        ////////////////////////////
        /// Get the MD5 corresponding to a process that is communicating out and check against the 
        /// downloaded EXEs
        //////////////////////
       
        public bool GetMD5FromPid(out String md5, uint pid)
        {
            md5 = String.Empty;
            try
            {
                if (pid == 0)  return false;

                ProcessEnumerator pn = new ProcessEnumerator();
                String procpath = String.Empty;
                bool b1 = pn.ProcessPathFromPid(out procpath, pid);
                if (b1 == true)
                {
                    ProxyMD5 m5 = new ProxyMD5();
                    String md5val = String.Empty;

                    bool b2 = m5.ComputeFileMD5(out md5val, procpath);
                    if (b2 == true && md5val != String.Empty)
                    {
                        md5 = md5val;
                        return true;
                    }

                }
            }
            catch (Exception e)
            {
                return false;
            }
            return false;
        }

        /// <summary>
        /// Pass MD5 / filesize and it will return matches
        /// </summary>
        
        public bool FindFileFromJournal(bool upload, uint size, String MD5, out List<string> matches)
        {
            matches = new List<string>();
            try
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                UsnJournal.NtfsUsnJournal nsf = new UsnJournal.NtfsUsnJournal(allDrives[0]);
                UInt64 MaximumSize = 0x800000;
                UInt64 AllocationDelta = 0x100000;
                List<UsnJournal.Win32Api.UsnEntry> ue = new List<UsnJournal.Win32Api.UsnEntry>();

                if (jsint == false)
                {
                     if (nsf.GetUsnJournalState(ref jns) ==
                          UsnJournal.NtfsUsnJournal.UsnJournalReturnCode.USN_JOURNAL_SUCCESS)
                     {
                         jsint = true;
                     }
                }

                if (jsint == true)
                {
                    UInt32 reason = (uint)(upload == true ?
                        UsnJournal.NtfsUsnJournal.UsnReasonCode.USN_REASON_BASIC_INFO_CHANGE :
                        UsnJournal.NtfsUsnJournal.UsnReasonCode.USN_REASON_FILE_CREATE);
                    nsf.GetUsnJournalEntries(jns,
                         (uint)reason, out ue,
                         out jns);
                    foreach (UsnJournal.Win32Api.UsnEntry el in ue)
                    {
                        String path = String.Empty;
                        nsf.GetPathFromFileReference(el.FileReferenceNumber, out path);

                        ProxyMD5 m5 = new ProxyMD5();
                        String md5val = String.Empty;

                        String pathtotal = allDrives[0].Name[0] + ":" + path;

                        bool b2 = m5.ComputeFileMD5(out md5val, pathtotal);
                        if (b2 == true && md5val != String.Empty)
                        {

                            Console.WriteLine(el.Name + " : " + md5val + " " + pathtotal);
                        }
                    }
                }
            }
            catch (Exception e)
            {
               
                return false;
            }



            return false;

        }




    }
}
