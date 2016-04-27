using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using System.IO;
using System.Collections.Concurrent;

namespace Forensics
{
    public class FileInfo
    {
        public string filepath;
        public UInt32 filesizelow;
        public UInt32 filesizehigh;
    }
    public class UsnThread
    {
        public bool terminate { get; set;}
        public Queue<UInt64> frefQueue;
        public ConcurrentDictionary <UInt64, FileInfo> finfo;
        bool jsint = false;
        UsnJournal.Win32Api.USN_JOURNAL_DATA jns;

        public bool GetDictionary(out ConcurrentDictionary<UInt64, FileInfo> fi)
        {
            fi = finfo;
            return true;
        }

        // This method that will be called when the thread is started
        public void UsnHandler()
        {
            try { 
                frefQueue = new Queue<UInt64>();
                finfo = new ConcurrentDictionary<UInt64, FileInfo>();
                jns = new UsnJournal.Win32Api.USN_JOURNAL_DATA();
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                UsnJournal.NtfsUsnJournal nsf = new UsnJournal.NtfsUsnJournal(allDrives[0]);
                String volumename = nsf.VolumeName;
                UInt64 MaximumSize = 0x800000;
                UInt64 AllocationDelta = 0x100000;
                while (true)
                {
                    if (terminate == true)
                        break;

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
                        List<UsnJournal.Win32Api.UsnEntry> ue = new List<UsnJournal.Win32Api.UsnEntry>();
                        UInt32 reason = (uint)(UsnJournal.NtfsUsnJournal.UsnReasonCode.USN_REASON_CLOSE);
                        nsf.GetUsnJournalEntries(jns,
                             (uint)reason, out ue,
                             out jns);
                        foreach (UsnJournal.Win32Api.UsnEntry el in ue)
                        {
                            if (frefQueue.Contains(el.FileReferenceNumber) == false)
                            {
                                frefQueue.Enqueue(el.FileReferenceNumber);
                                uint filesize = 0;
                                string filename = string.Empty;
                                string fullfilename = string.Empty;

                                nsf.GetPathFromFileReference(el.FileReferenceNumber, out filename);
                                if (filename != null && filename.Length > 1 && (filename[0] == '\\' || filename[0] == '/'))
                                {
                                    fullfilename = volumename + filename.Substring(1);
                                }
                                else
                                {
                                    fullfilename = volumename + filename;
                                }
                                nsf.GetSizeFromFileReference(el.FileReferenceNumber, out filesize);

                                FileInfo fi = new FileInfo();

                                fi.filepath = fullfilename;
                                fi.filesizelow = filesize;
                                finfo.TryAdd(el.FileReferenceNumber, fi);
                               // Console.WriteLine(fullfilename + "  :  " + filesize + " : " +  el.FileReferenceNumber.ToString());

                                if (frefQueue.Count > ConstantVariables.MAX_USN_QUEUE)
                                {
                                    UInt64 lkey = frefQueue.Dequeue();
                                    FileInfo fn;
                                    finfo.TryRemove(lkey, out fn);
                                }
                            }           
                        }
                    }

                    for (int i = 0; i < 5; i++)
                    {
                        Thread.Sleep(1000);
                        if (terminate == true)
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                
            } 
        }
    }

    public class UsnWatch
    {
        UsnThread usnThread;
        Thread oThread;

        private System.Threading.Mutex mut = new System.Threading.Mutex();

        public bool StartUsnWatch()
        {
            try
            {
                usnThread = new UsnThread();

                oThread = new Thread(new ThreadStart(usnThread.UsnHandler));

                oThread.Start();

            }
            catch (Exception ex)
            {
                return false;
            }

            return false;
        }

        public bool StopUsnWatch()
        {
            try
            {
                usnThread.terminate = true;
                if (oThread != null)
                {
                    oThread.Join(2000);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public bool CrossCheckMD5(string md5, UInt32 filesize, string filetype, out List<string> filepaths)
        {
            bool retval = false;
            ConcurrentDictionary<UInt64, FileInfo> fi;
            filepaths = new List<string>();
            try 
            {
                mut.WaitOne();
                if (usnThread != null)
                {
                    usnThread.GetDictionary(out fi);
                    if (fi != null)
                    {
                        foreach(KeyValuePair<UInt64, FileInfo> ff in fi) {
                            FileInfo ffval = ff.Value;
                            if (ffval.filesizelow == filesize) //only 32bit size is considered
                            {
                                ProxyMD5 md5v = new ProxyMD5();
                                string md5val = string.Empty;
                                if (md5v.ComputeFileMD5(out md5val, ffval.filepath) == true)
                                {
                                    if (md5val != null)
                                    {
                                        if (md5.Equals(md5val, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            filepaths.Add(ffval.filepath);
                                        }
                                    }
                                }
                            }
                        }
                        if (filepaths.Count > 0)
                        {
                            retval =  true;
                        }
                    }
                }
            } 
            catch (Exception ex)
            {

            }
            finally 
            {
                mut.ReleaseMutex();
            }

            return retval;
        }

    }
}
