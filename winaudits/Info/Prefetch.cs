using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Management;

namespace winaudits
{
    public class Prefetch
    {
        [JsonProperty("filename")]
        public string FileName { get; set; }
        [JsonProperty("fullpath")]
        public string FullPath { get; set; }
        [JsonProperty("prefetchpath")]
        public List<string> PrefetchPath { get; set; }
        [JsonProperty("lastrun")]
        public string LastRun { get; set; }
        [JsonProperty("created")]
        public string Created { get; set; }
        [JsonProperty("timesrun")]
        public int TimesRun { get; set; }
        [JsonProperty("size")]
        public int Size { get; set; }
        [JsonProperty("hashofprefetch")]
        public string HashOfPrefetch { get; set; }
    }

    public static class PrefetchAuditor
    {
        private static UInt64 PREFETCH_HDR_VISTA = 0x4143435300000017;
        private static UInt64 PREFETCH_HDR_WIN8 = 0x414343530000001a;
        private static UInt64 PREFETCH_HDR_WIN10 = 0x414343530000001e;

        [StructLayout(LayoutKind.Sequential)]
        private struct FILETIME
        {
            public uint DateTimeLow;
            public uint DateTimeHigh;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct PrefetchFileVista
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] uk3;
            public FILETIME lastExecuted;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] uk4;
            public UInt32 numExecuted;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct PrefetchFileWin8
        {
            public UInt32 volInfoLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] uk3;
            public FILETIME lastExecuted;
            public FILETIME lastExec1;
            public FILETIME lastExec2;
            public FILETIME lastExec3;
            public FILETIME lastExec4;
            public FILETIME lastExec5;
            public FILETIME lastExec6;
            public FILETIME lastExec7;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] uk2;
            public UInt32 numExecuted;

        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct PrefetchBaseFileWin8
        {
            public UInt64 magic;
            public UInt32 uk1;
            public UInt32 fileSize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
            public char[] name;
            public UInt32 pathsOffset;
            public UInt32 pathsLen;
            public UInt32 volOffset;
            public UInt32 numVols;
            public PrefetchFileWin8 pref8;

        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
        private struct PrefetchBaseFileWin7
        {
            public UInt64 magic;
            public UInt32 uk1;
            public UInt32 fileSize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
            public char[] name;
            public UInt32 pathsOffset;
            public UInt32 pathsLen;
            public UInt32 volOffset;
            public UInt32 numVols;
            public PrefetchFileVista pref7;
        }

        [DllImport("kernel32.dll")]
        static extern uint QueryDosDevice(string lpDeviceName, IntPtr lpTargetPath, uint ucchMax);

        private static string GetPrefetchDir(bool is64)
        {
            string val = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            if (string.IsNullOrWhiteSpace(val))
            {
                return null;
            }
            val = val + "\\" + "Prefetch";

            return val;
        }


        [DllImport("mpr.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int WNetGetConnection([MarshalAs(UnmanagedType.LPTStr)] string localName, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder remoteName, int length);

        private static PrefetchBaseFileWin7 ByteArrayToPrefetchWin7(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            PrefetchBaseFileWin7 stuff = (PrefetchBaseFileWin7)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(PrefetchBaseFileWin7));
            handle.Free();
            return stuff;
        }

        private static PrefetchBaseFileWin8 ByteArrayToPrefetchWin8(byte[] bytes)
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            PrefetchBaseFileWin8 stuff = (PrefetchBaseFileWin8)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(PrefetchBaseFileWin8));
            handle.Free();
            return stuff;
        }

        private static string PrefetchFilePath(byte[] bytes, int offset, int length)
        {
            string s3 = "";
            try
            {
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(length);
                Marshal.Copy(bytes, offset, unmanagedPointer, length);
                s3 = (string)Marshal.PtrToStringUni(unmanagedPointer);
                Marshal.FreeHGlobal(unmanagedPointer);
            }
            catch (Exception)
            {
                return null;
            }
            return s3;
        }

        public static List<Prefetch> StartAudit()
        {

            Dictionary<string, string> driveInfo = new Dictionary<string, string>();

            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (DriveInfo drive in drives)
            {

                uint returnSize = 0;
                uint maxSize = 100;
                string allDevices = null;
                IntPtr mem;
                string retval = null;


                mem = Marshal.AllocHGlobal((int)maxSize);
                if (mem != IntPtr.Zero)
                {
                    // mem points to memory that needs freeing
                    try
                    {
                        returnSize = QueryDosDevice(drive.Name.TrimEnd('\\'), mem, maxSize);
                        if (returnSize != 0)
                        {
                            allDevices = Marshal.PtrToStringAnsi(mem, (int)returnSize);
                            retval = allDevices.Replace("\0", string.Empty);
                            driveInfo.Add(retval.ToUpper(), drive.Name.TrimEnd('\\'));
                        }
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(mem);
                    }
                }
            }


            List<Prefetch> pfl = new List<Prefetch>();
            try
            {
                string predir = GetPrefetchDir(false);
                if (predir == null)
                {
                    return null;
                }

                string[] prefiles = Directory.GetFiles(predir);
                for (int i = 0; i < prefiles.Length; i++)
                {

                    var fs = new FileStream(prefiles[i], FileMode.Open, FileAccess.Read, FileShare.Read);
                    if (fs != null)
                    {
                        int len = (int)fs.Length;
                        if (len > 100 && len < 1000000)
                        {
                            byte[] buff1 = new byte[len];
                            int l2 = fs.Read(buff1, 0, len);
                            if (l2 > 100)
                            {
                                UInt64 bc = BitConverter.ToUInt64(buff1, 0);
                                Prefetch prf = new Prefetch();
                                if (bc == PREFETCH_HDR_VISTA)
                                {
                                    PrefetchBaseFileWin7 pf7 = ByteArrayToPrefetchWin7(buff1);
                                    {
                                        uint pathlen = pf7.pathsLen;
                                        uint pathoffset = pf7.pathsOffset;


                                        var filenameStringsBytes = new byte[pf7.pathsLen];
                                        Buffer.BlockCopy(buff1, (int)pathoffset, filenameStringsBytes, 0, (int)pathlen);

                                        var filenamesRaw = Encoding.Unicode.GetString(filenameStringsBytes);
                                        prf.PrefetchPath = filenamesRaw.Split(new Char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries).ToList();


                                        byte[] highBytes = BitConverter.GetBytes(pf7.pref7.lastExecuted.DateTimeHigh);
                                        // Resize the array to 8 bytes (for a Long)
                                        Array.Resize(ref highBytes, 8);

                                        long returnedLong = BitConverter.ToInt64(highBytes, 0);
                                        returnedLong = returnedLong << 32;

                                        returnedLong = returnedLong | pf7.pref7.lastExecuted.DateTimeLow;
                                        prf.LastRun = DateTime.FromFileTimeUtc(returnedLong).ToString();

                                        prf.FullPath = prefiles[i];
                                        prf.Size = (int)pf7.fileSize;
                                        prf.TimesRun = (int)pf7.pref7.numExecuted;
                                    }
                                }
                                else
                                {
                                    if (bc == PREFETCH_HDR_WIN8)
                                    {
                                        PrefetchBaseFileWin8 pf8 = ByteArrayToPrefetchWin8(buff1);
                                        {
                                            uint pathlen = pf8.pathsLen;
                                            uint pathoffset = pf8.pathsOffset;

                                            var filenameStringsBytes = new byte[pathlen];
                                            Buffer.BlockCopy(buff1, (int)pathoffset, filenameStringsBytes, 0, (int)pathlen);

                                            var filenamesRaw = Encoding.Unicode.GetString(filenameStringsBytes);
                                            prf.PrefetchPath = filenamesRaw.Split(new Char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                                            long tm1 = pf8.pref8.lastExecuted.DateTimeHigh << 32;
                                            long time2 = tm1 + pf8.pref8.lastExecuted.DateTimeLow;
                                            prf.LastRun = DateTime.FromFileTime(time2).ToString();
                                            prf.FullPath = prefiles[i];
                                            prf.Size = (int)pf8.fileSize;
                                        }
                                    }
                                    else
                                    {
                                        //For windows 10
                                        var tempSig = Encoding.ASCII.GetString(buff1, 0, 3);
                                        if (tempSig.Equals("MAM"))
                                        {
                                            //windows 10, so we need to decompress
                                            //Size of decompressed data is at offset 4
                                            var size = BitConverter.ToUInt32(buff1, 4);

                                            //get our compressed bytes (skipping signature and uncompressed size)
                                            var compressedBytes = buff1.Skip(8).ToArray();
                                            var decom = Xpress2.Decompress(compressedBytes, size);

                                            if (decom == null)
                                            {
                                                continue;
                                            }
                                            //update rawBytes with decompressed bytes so the rest works
                                            buff1 = decom;
                                        }

                                        var bc32 = BitConverter.ToInt32(buff1, 0);
                                        if (bc32 == 30)
                                        {
                                            var fileInfoBytes = new byte[224];
                                            Buffer.BlockCopy(buff1, 84, fileInfoBytes, 0, 224);

                                            int pathoffset = BitConverter.ToInt32(fileInfoBytes, 16);
                                            int pathlen = BitConverter.ToInt32(fileInfoBytes, 20);
                                            var filenameStringsBytes = new byte[pathlen];
                                            Buffer.BlockCopy(buff1, (int)pathoffset, filenameStringsBytes, 0, (int)pathlen);

                                            var filenamesRaw = Encoding.Unicode.GetString(filenameStringsBytes);
                                            prf.PrefetchPath = filenamesRaw.Split(new Char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                                            var runtimeBytes = new byte[64];
                                            Buffer.BlockCopy(fileInfoBytes, 44, runtimeBytes, 0, 64);
                                            var rawTime = BitConverter.ToInt64(runtimeBytes, 0);

                                            if (rawTime > 0)
                                            {
                                                prf.LastRun = DateTimeOffset.FromFileTime(rawTime).ToUniversalTime().ToString();
                                            }

                                            prf.TimesRun = BitConverter.ToInt32(fileInfoBytes, 124);
                                            prf.FullPath = prefiles[i];
                                            prf.FileName = Path.GetFileNameWithoutExtension(prf.FullPath);
                                        }
                                    }
                                }

                                if (!string.IsNullOrEmpty(prf.FullPath))
                                {
                                    var fi = new FileInfo(prefiles[i]);
                                    prf.FileName = fi.Name;
                                    prf.Created = new DateTimeOffset(fi.CreationTimeUtc).ToString("yyyy-MM-dd HH:mm:ss.fff");
                                    prf.HashOfPrefetch = Forensics.ProxyMD5.ComputeFileMD5(prf.FullPath);
                                    if (prf.Size == 0)
                                    {
                                        prf.Size = (int)fi.Length;
                                    }

                                    List<string> tempPaths = new List<string>(prf.PrefetchPath);
                                    prf.PrefetchPath.Clear();
                                    foreach (var item in tempPaths)
                                    {
                                        int third = GetNthIndex(item, '\\', 3);
                                        string subString = item.Substring(0, third);

                                        if (driveInfo.ContainsKey(subString.ToUpper()))
                                        {
                                            prf.PrefetchPath.Add(item.Replace(subString, driveInfo[subString]));
                                        }
                                        else
                                        {
                                            prf.PrefetchPath.Add(item);
                                        }
                                    }
                                    pfl.Add(prf);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

            return pfl;
        }

        public static int GetNthIndex(string s, char t, int n)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == t)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
    }
}
