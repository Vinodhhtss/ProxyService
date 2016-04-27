using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace winaudits
{
    // ref: http://www.microsoft.com/whdc/system/Sysinternals/MoreThan64proc.mspx
    enum PROCESSINFOCLASS : int
    {
        ProcessBasicInformation = 0, // 0, q: PROCESS_BASIC_INFORMATION, PROCESS_EXTENDED_BASIC_INFORMATION
        ProcessWow64Information = 26, // q: ULONG_PTR
    }

    [Flags]
    enum PEB_OFFSET
    {
        CurrentDirectory,
        //DllPath,
        //ImagePathName,
        CommandLine,
        //WindowTitle,
        //DesktopInfo,
        //ShellInfo,
        //RuntimeData,
        //TypeMask = 0xffff,
        //Wow64 = 0x10000,
    }

    public class LoadedModule
    {
        [JsonProperty("processid")]
        public int ProcessId { get; set; }
        [JsonProperty("modulename")]
        public string ModuleName { get; set; }
        [JsonProperty("modulepath")]
        public string ModulePath { get; set; }
        [JsonProperty("md5")]
        public string MD5 { get; set; }
        [JsonProperty("issigned")]
        public bool IsSigned { get; set; }
        [JsonProperty("isverified")]
        public bool IsVerified { get; set; }
        [JsonProperty("signaturestring")]
        public string SignatureString { get; set; }
        [JsonProperty("ca")]
        public string CA { get; set; }
        [JsonProperty("certsubject")]
        public string CertSubject { get; set; }
    }

    public class RunningProcess
    {
        [JsonProperty("dbid")]
        public int ID { get; set; }
        [JsonProperty("processname")]
        public string ProcessName { get; set; }
        [JsonProperty("pid")]
        public UInt32 PID { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("arguments")]
        public string Arguments { get; set; }
        [JsonProperty("starttime")]
        public DateTime StartTime { get; set; }
        [JsonProperty("kernaltime")]
        public string KernalTime { get; set; }
        [JsonProperty("usertime")]
        public string UserTime { get; set; }
        [JsonProperty("workingset")]
        public int WorkingSet { get; set; }
        [JsonProperty("username")]
        public string UserName { get; set; }
        [JsonProperty("ishidden")]
        public bool IsHidden { get; set; }
        [JsonProperty("sid")]
        public string SID { get; set; }
        [JsonProperty("sidtype")]
        public string SIDType { get; set; }
        [JsonProperty("parentname")]
        public string ParentName { get; set; }
        [JsonProperty("ppid")]
        public UInt32 PPID { get; set; }
        [JsonProperty("md5")]
        public string MD5 { get; set; }
        [JsonProperty("issigned")]
        public bool IsSigned { get; set; }
        [JsonProperty("isverified")]
        public bool IsVerified { get; set; }
        [JsonProperty("signaturestring")]
        public string SignatureString { get; set; }
        [JsonProperty("ca")]
        public string CA { get; set; }
        [JsonProperty("certsubject")]
        public string CertSubject { get; set; }
        [JsonProperty("iswow64")]
        public bool IsWow64 { get; set; }
        public List<LoadedModule> ListProcessModule { get; set; }
    }

    static class LookupAccountName
    {
        public const UInt32 STANDARD_RIGHTS_READ = 0x00020000;
        public const UInt32 TOKEN_QUERY = 0x0008;
        public const UInt32 TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool OpenProcessToken(IntPtr ProcessHandle,
            UInt32 DesiredAccess, out IntPtr TokenHandle);

        public struct TOKEN_USER
        {
            public SID_AND_ATTRIBUTES User;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SID_AND_ATTRIBUTES
        {

            public IntPtr Sid;
            public int Attributes;
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength, out uint ReturnLength);

        public enum TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin
        }

        [DllImport("kernel32.dll")]
        static extern uint GetLastError();

        enum SID_NAME_USE
        {
            SidTypeUser = 1,
            SidTypeGroup,
            SidTypeDomain,
            SidTypeAlias,
            SidTypeWellKnownGroup,
            SidTypeDeletedAccount,
            SidTypeInvalid,
            SidTypeUnknown,
            SidTypeComputer
        }


        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool LookupAccountSid(
            string lpSystemName,
            [MarshalAs(UnmanagedType.LPArray)] byte[] Sid,
            System.Text.StringBuilder lpName,
            ref uint cchName,
            System.Text.StringBuilder ReferencedDomainName,
            ref uint cchReferencedDomainName,
            out SID_NAME_USE peUse);

        private static string GetUserNameFromSID(byte[] sid)
        {
            StringBuilder name = new StringBuilder();
            uint cchName = (uint)name.Capacity;
            StringBuilder referencedDomainName = new StringBuilder();
            uint cchReferencedDomainName = (uint)referencedDomainName.Capacity;
            SID_NAME_USE sidUse;
            // Sid for BUILTIN\Administrators

            byte[] Sid = new byte[] { 1, 2, 0, 0, 0, 0, 0, 5, 32, 0, 0, 0, 32, 2 };

            if (!LookupAccountSid(null, sid, name, ref cchName, referencedDomainName, ref cchReferencedDomainName, out sidUse))
            {
                return null;
            }

            return name.ToString();
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static string GetProcessUser(IntPtr hProcess, RunningProcess pre)
        {
            int MAX_INTPTR_BYTE_ARR_SIZE = 512;
            IntPtr tokenHandle;
            byte[] sidBytes;

            try
            {
                winaudits.AdjustPrevilege.AddDebugPrevilege();

                // Get the Process Token
                if (!OpenProcessToken(hProcess, TOKEN_READ, out tokenHandle))
                {
                    return null;
                }

                uint tokenInfoLength = 0;
                bool result;
                result = GetTokenInformation(tokenHandle, TOKEN_INFORMATION_CLASS.TokenUser, IntPtr.Zero, tokenInfoLength, out tokenInfoLength);

                // get the token info length
                IntPtr tokenInfo = Marshal.AllocHGlobal((int)tokenInfoLength);
                result = GetTokenInformation(tokenHandle, TOKEN_INFORMATION_CLASS.TokenUser, tokenInfo, tokenInfoLength, out tokenInfoLength);  // get the token info

                // Get the User SID
                if (result)
                {
                    TOKEN_USER tokenUser = (TOKEN_USER)Marshal.PtrToStructure(tokenInfo, typeof(TOKEN_USER));
                    sidBytes = new byte[MAX_INTPTR_BYTE_ARR_SIZE];  // Since I don't yet know how to be more precise w/ the size of the byte arr, it is being set to 512
                    Marshal.Copy(tokenUser.User.Sid, sidBytes, 0, MAX_INTPTR_BYTE_ARR_SIZE);  // get a byte[] representation of the SID


                    var sid = new SecurityIdentifier(sidBytes, 0);
                    if (sid != null)
                    {
                        pre.SID = sid.ToString();
                        if (sid.IsAccountSid() == true)
                        {
                            pre.SIDType = "Account SID";
                        }
                        else
                        {
                            pre.SIDType = "WellKnown";
                        }
                    }

                    return GetUserNameFromSID(sidBytes);
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }
    }


    // All offset values below have been tested on Windows 7 & 8 only
    // but you can use WinDbg "dt ntdll!_PEB" command and search for ProcessParameters offset to find the truth, depending on the OS version
    public static class ProcessList
    {
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        private struct ListEntryWrapper
        {
            public LIST_ENTRY Header;
            public LDR_MODULE Body;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        private struct LDR_MODULE
        {
            public LIST_ENTRY InLoadOrderModuleList;
            public LIST_ENTRY InMemoryOrderModuleList;
            public LIST_ENTRY InInitializationOrderModuleList;
            IntPtr BaseAddress;
            IntPtr EntryPoint;
            UInt32 SizeOfImage;
            public UNICODE_STRING FullDllName;
            public UNICODE_STRING BaseDllName;
            UInt32 Flags;
            Int16 LoadCount;
            Int16 TlsIndex;
            LIST_ENTRY HashTableEntry;
            UInt32 TimeDateStamp;
        }

        private static T ReadMemory<T>(this IntPtr atAddress)
        {
            var ret = (T)Marshal.PtrToStructure(atAddress, typeof(T));
            return ret;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        private struct LIST_ENTRY
        {
            public IntPtr Flink;
            public IntPtr Blink;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        private struct PEB_LDR_DATA
        {
            public int Length;
            public int Initialized;
            public int SsHandle;
            public IntPtr InLoadOrderModuleListPtr;   //0x0c && 0x10
            public IntPtr InMemoryOrderModuleListPtr;
            public IntPtr InInitOrderModuleListPtr;
            public int EntryInProgress;
            public ListEntryWrapper InLoadOrderModuleList { get { return InLoadOrderModuleListPtr.ReadMemory<ListEntryWrapper>(); } }
            public ListEntryWrapper InMemoryOrderModuleList { get { return InLoadOrderModuleListPtr.ReadMemory<ListEntryWrapper>(); } }
            public ListEntryWrapper InInitOrderModuleList { get { return InLoadOrderModuleListPtr.ReadMemory<ListEntryWrapper>(); } }
        }


        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle([In] IntPtr hObject);

        private static readonly bool Is64BitProcess = IntPtr.Size > 4;
        private static readonly bool Is64BitOperatingSystem = Is64BitProcess || PlatformCheck.InternalCheckIsWow64();


        private static IntPtr StructToPtr(object obj)
        {
            var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(obj));
            Marshal.StructureToPtr(obj, ptr, false);
            return ptr;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FILETIME
        {
            public uint DateTimeLow;
            public uint DateTimeHigh;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetProcessTimes(IntPtr hProcess, out FILETIME
           lpCreationTime, out FILETIME lpExitTime, out FILETIME lpKernelTime,
           out FILETIME lpUserTime);


        /// <summary>
        // Process Memory Counters. Working set
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = 40)]
        private struct PROCESS_MEMORY_COUNTERS
        {
            public uint cb;
            public uint PageFaultCount;
            public uint PeakWorkingSetSize;
            public uint WorkingSetSize;
            public uint QuotaPeakPagedPoolUsage;
            public uint QuotaPagedPoolUsage;
            public uint QuotaPeakNonPagedPoolUsage;
            public uint QuotaNonPagedPoolUsage;
            public uint PagefileUsage;
            public uint PeakPagefileUsage;
        }

        [DllImport("psapi.dll", SetLastError = true)]
        static extern bool GetProcessMemoryInfo(IntPtr hProcess, out PROCESS_MEMORY_COUNTERS counters, uint size);

        private static string ConvertTimeToString(uint high, uint low, bool flag)
        {
            ulong high1 = high;
            unchecked
            {

                uint uLow = (uint)low;
                high1 = high1 << 32;
                DateTime dt = DateTime.FromFileTime((long)(high1 | (ulong)uLow));

                if (flag == true)
                {
                    return dt.ToString();
                }
                else
                {
                    return dt.ToLongTimeString();
                }
            }
        }

        private static string GetProcessParametersString(int processId, PEB_OFFSET Offset, RunningProcess pre)
        {
            IntPtr handle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, processId);
            if (handle == IntPtr.Zero)
                return null;

            ///////////////////// Get the username associated with the process ///////////////////

            string username = LookupAccountName.GetProcessUser(handle, pre);
            if (username != null)
            {
                pre.UserName = username;
            }
            /////////////////////////////////////////////////////////////////////////////////////



            /////////////////// Get Working Set Counters ///////////////////////////////////////
            PROCESS_MEMORY_COUNTERS memoryCounters;

            memoryCounters.cb = (uint)Marshal.SizeOf(typeof(PROCESS_MEMORY_COUNTERS));
            if (GetProcessMemoryInfo(handle, out memoryCounters, memoryCounters.cb))
            {
                pre.WorkingSet = (int)memoryCounters.WorkingSetSize;
                //pre.workingsetstr = pre.workingset.ToString("#,##0") + " " + "Bytes";
            }

            ////////////////////////////////////////////////////////////////////////////////////

            FILETIME lpCreationTime = new FILETIME();
            FILETIME lpUserTime = new FILETIME();
            FILETIME lpKernelTime = new FILETIME();
            FILETIME lpExitTime = new FILETIME();

            bool retval = GetProcessTimes(handle, out lpCreationTime, out lpExitTime, out lpKernelTime, out lpUserTime);

            if (retval == true)
            {
                string sc = ConvertTimeToString(lpCreationTime.DateTimeHigh, lpCreationTime.DateTimeLow, true);
                pre.StartTime = DateTime.Parse(sc);

                string kr = ConvertTimeToString(lpKernelTime.DateTimeHigh, lpKernelTime.DateTimeLow, false);
                pre.KernalTime = kr;

                string ut = ConvertTimeToString(lpUserTime.DateTimeHigh, lpUserTime.DateTimeLow, false);
                pre.UserTime = ut;
            }

            bool IsWow64Process = PlatformCheck.InternalCheckIsWow64();
            bool IsTargetWow64Process = PlatformCheck.GetProcessIsWow64(handle);
            bool IsTarget64BitProcess = Is64BitOperatingSystem && !IsTargetWow64Process;

            pre.IsWow64 = IsTargetWow64Process;

            long offset = 0;
            long processParametersOffset = IsTarget64BitProcess ? 0x20 : 0x10;
            long offsetldr = 0x0c;

            switch (Offset)
            {
                case PEB_OFFSET.CurrentDirectory:
                    offset = IsTarget64BitProcess ? 0x38 : 0x24;
                    break;
                case PEB_OFFSET.CommandLine:
                    offset = Is64BitOperatingSystem ? 0x70 : 0x40;
                    if (offset == 0x70 && IsTargetWow64Process)
                    {
                        offset = 0x40;
                    }
                    break;
                default:
                    return null;
            }

            try
            {
                long pebAddress = 0;
                if (IsTargetWow64Process) // OS : 64Bit, Cur : 32 or 64, Tar: 32bit
                {
                    IntPtr peb32 = new IntPtr();

                    int hr = NtQueryInformationProcess(handle, (int)PROCESSINFOCLASS.ProcessWow64Information, ref peb32, IntPtr.Size, IntPtr.Zero);
                    if (hr != 0) return null;
                    pebAddress = peb32.ToInt64();

                    IntPtr pp = new IntPtr();
                    if (!ReadProcessMemory(handle, new IntPtr(pebAddress + processParametersOffset), ref pp, new IntPtr(Marshal.SizeOf(pp)), IntPtr.Zero))
                        return null;

                    UNICODE_STRING_32 us = new UNICODE_STRING_32();
                    if (!ReadProcessMemory(handle, new IntPtr(pp.ToInt64() + offset), ref us, new IntPtr(Marshal.SizeOf(us)), IntPtr.Zero))
                        return null;

                    if ((us.Buffer == 0) || (us.Length == 0))
                        return null;

                    string s = new string('\0', us.Length / 2);
                    if (!ReadProcessMemory(handle, new IntPtr(us.Buffer), s, new IntPtr(us.Length), IntPtr.Zero))
                        return null;

                    UNICODE_STRING_32 us2 = new UNICODE_STRING_32();
                    if (!ReadProcessMemory(handle, new IntPtr(pp.ToInt64() + offset - 8), ref us2, new IntPtr(Marshal.SizeOf(us2)), IntPtr.Zero))
                        return null;

                    if ((us2.Buffer == 0) || (us2.Length == 0))
                        return null;

                    string s2 = new string('\0', us2.Length / 2);
                    if (!ReadProcessMemory(handle, new IntPtr(us2.Buffer), s2, new IntPtr(us2.Length), IntPtr.Zero))
                        return null;

                    pre.Arguments = s;
                    pre.Path = s2;

                    //////////// Read Loader ////////
                    PEB_LDR_DATA ldr1 = new PEB_LDR_DATA();
                    IntPtr ldrp = StructToPtr(ldr1);
                    if (!ReadProcessMemory(handle, new IntPtr(pebAddress + 0x0c), ref ldrp, new IntPtr(Marshal.SizeOf(ldr1)), IntPtr.Zero))
                        return null;

                    /// We have 
                    long ldraddress = ldrp.ToInt64();
                    IntPtr le1 = new IntPtr(ldraddress + 0x0c);
                    if (!ReadProcessMemory(handle, new IntPtr(ldraddress + 0xc), ref le1, new IntPtr(Marshal.SizeOf(le1)), IntPtr.Zero))
                        return null;

                    while (le1 != null)
                    {

                        UNICODE_STRING_32 us3 = new UNICODE_STRING_32();
                        if (!ReadProcessMemory(handle, new IntPtr(le1.ToInt64() + 0x24), ref us3, new IntPtr(Marshal.SizeOf(us3)), IntPtr.Zero))
                            return null;

                        if ((us3.Buffer == 0) || (us3.Length == 0))
                            return null;

                        string s3 = new string('\0', us3.Length / 2);
                        if (!ReadProcessMemory(handle, new IntPtr(us3.Buffer), s3, new IntPtr(us3.Length), IntPtr.Zero))
                            return null;


                        long nextlist = le1.ToInt64();
                        IntPtr nextlistp = new IntPtr(nextlist);
                        if (!ReadProcessMemory(handle, nextlistp, ref le1, new IntPtr(Marshal.SizeOf(le1)), IntPtr.Zero))
                            return null;

                        LoadedModule pme = new LoadedModule();
                        pme.ModulePath = s3;
                        pre.ListProcessModule.Add(pme);

                    }

                    return s;
                }
                else if (IsWow64Process)//Os : 64Bit, Cur 32, Tar 64
                {
                    PROCESS_BASIC_INFORMATION_WOW64 pbi = new PROCESS_BASIC_INFORMATION_WOW64();
                    int hr = NtWow64QueryInformationProcess64(handle, (int)PROCESSINFOCLASS.ProcessBasicInformation, ref pbi, Marshal.SizeOf(pbi), IntPtr.Zero);
                    if (hr != 0) return null;
                    pebAddress = pbi.PebBaseAddress;

                    long pp = 0;
                    hr = NtWow64ReadVirtualMemory64(handle, pebAddress + processParametersOffset, ref pp, Marshal.SizeOf(pp), IntPtr.Zero);
                    if (hr != 0)
                        return null;

                    UNICODE_STRING_WOW64 us = new UNICODE_STRING_WOW64();
                    hr = NtWow64ReadVirtualMemory64(handle, pp + offset, ref us, Marshal.SizeOf(us), IntPtr.Zero);
                    if (hr != 0)
                        return null;

                    if ((us.Buffer == 0) || (us.Length == 0))
                        return null;

                    string s = new string('\0', us.Length / 2);
                    hr = NtWow64ReadVirtualMemory64(handle, us.Buffer, s, us.Length, IntPtr.Zero);
                    if (hr != 0)
                        return null;

                    if (pre != null)
                    {
                        pre.Arguments = s;
                    }

                    UNICODE_STRING_WOW64 us2 = new UNICODE_STRING_WOW64();
                    hr = NtWow64ReadVirtualMemory64(handle, pp + offset - Marshal.SizeOf(us2), ref us2, Marshal.SizeOf(us2), IntPtr.Zero);
                    if (hr != 0)
                        return null;

                    if ((us2.Buffer == 0) || (us2.Length == 0))
                        return null;

                    string s2 = new string('\0', us2.Length / 2);
                    hr = NtWow64ReadVirtualMemory64(handle, us2.Buffer, s2, us2.Length, IntPtr.Zero);
                    if (hr != 0)
                        return null;

                    // Console.WriteLine("IM : " + s2);
                    pre.Path = s2;

                    /////////////////////For Testing
                    if (s2 != null && ((s2.ToLower().Contains("taskmgr.exe") == false)))
                    {
                        //return s2;
                    }
                    //////////// Read Loader ////////

                    long ldrp = 0;
                    if (NtWow64ReadVirtualMemory64(handle, pebAddress + 0x18, ref ldrp, sizeof(long), IntPtr.Zero) != 0)
                        return null;


                    /// We have 
                    long ldraddress = ldrp + 0x10;
                    long le1 = 0;
                    if (NtWow64ReadVirtualMemory64(handle, ldraddress, ref le1, sizeof(long), IntPtr.Zero) != 0)
                        return null;

                    while (le1 != 0)
                    {

                        UNICODE_STRING_WOW64 us3 = new UNICODE_STRING_WOW64();
                        hr = NtWow64ReadVirtualMemory64(handle, le1 + 0x48, ref us3, Marshal.SizeOf(us3), IntPtr.Zero);
                        if (hr != 0)
                            return null;

                        if ((us3.Buffer == 0) || (us3.Length == 0))
                            return null;

                        string s3 = new string('\0', us3.Length / 2);
                        hr = NtWow64ReadVirtualMemory64(handle, us3.Buffer, s3, us3.Length, IntPtr.Zero);
                        if (hr != 0)
                            return null;

                        long nextlist = le1;
                        hr = NtWow64ReadVirtualMemory64(handle, nextlist, ref le1, sizeof(long), IntPtr.Zero);
                        if (hr != 0)
                            return null;

                        LoadedModule pme = new LoadedModule();
                        pme.ModulePath = s3;
                        pre.ListProcessModule.Add(pme);
                    }

                    return s;
                }
                else// Os,Cur,Tar : 64 or 32
                {
                    PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
                    int hr = NtQueryInformationProcess(handle, (int)PROCESSINFOCLASS.ProcessBasicInformation, ref pbi, Marshal.SizeOf(pbi), IntPtr.Zero);
                    if (hr != 0) return null;
                    pebAddress = pbi.PebBaseAddress.ToInt64();

                    IntPtr pp = new IntPtr();
                    if (!ReadProcessMemory(handle, new IntPtr(pebAddress + processParametersOffset), ref pp, new IntPtr(Marshal.SizeOf(pp)), IntPtr.Zero))
                        return null;

                    UNICODE_STRING us = new UNICODE_STRING();
                    if (!ReadProcessMemory(handle, new IntPtr((long)pp + offset), ref us, new IntPtr(Marshal.SizeOf(us)), IntPtr.Zero))
                        return null;

                    if ((us.Buffer == IntPtr.Zero) || (us.Length == 0))
                        return null;

                    string s = new string('\0', us.Length / 2);
                    if (!ReadProcessMemory(handle, us.Buffer, s, new IntPtr(us.Length), IntPtr.Zero))
                        return null;

                    UNICODE_STRING us2 = new UNICODE_STRING();
                    if (!ReadProcessMemory(handle, new IntPtr((long)pp + offset - Marshal.SizeOf(us2)), ref us2, new IntPtr(Marshal.SizeOf(us2)), IntPtr.Zero))
                        return null;

                    if ((us2.Buffer == IntPtr.Zero) || (us2.Length == 0))
                        return null;

                    string s2 = new string('\0', us2.Length / 2);
                    if (!ReadProcessMemory(handle, us2.Buffer, s2, new IntPtr(us2.Length), IntPtr.Zero))
                        return null;

                    pre.Path = s2;
                    pre.Arguments = s;

                    //////////// Read Loader ////////
                    PEB_LDR_DATA ldr1 = new PEB_LDR_DATA();
                    IntPtr ldrp = StructToPtr(ldr1);
                    if (!ReadProcessMemory(handle, new IntPtr(pebAddress + 0x0c), ref ldrp, new IntPtr(Marshal.SizeOf(ldr1)), IntPtr.Zero))
                        return null;


                    /// We have 
                    long ldraddress = ldrp.ToInt64();
                    IntPtr le1 = new IntPtr(ldraddress + 0x0c);
                    if (!ReadProcessMemory(handle, new IntPtr(ldraddress + 0xc), ref le1, new IntPtr(Marshal.SizeOf(le1)), IntPtr.Zero))
                        return null;

                    while (le1 != null)
                    {

                        UNICODE_STRING_32 us3 = new UNICODE_STRING_32();
                        if (!ReadProcessMemory(handle, new IntPtr(le1.ToInt64() + 0x24), ref us3, new IntPtr(Marshal.SizeOf(us3)), IntPtr.Zero))
                            return null;

                        if ((us3.Buffer == 0) || (us3.Length == 0))
                            return null;

                        string s3 = new string('\0', us3.Length / 2);
                        if (!ReadProcessMemory(handle, new IntPtr(us3.Buffer), s3, new IntPtr(us3.Length), IntPtr.Zero))
                            return null;


                        long nextlist = le1.ToInt64();
                        IntPtr nextlistp = new IntPtr(nextlist);
                        if (!ReadProcessMemory(handle, nextlistp, ref le1, new IntPtr(Marshal.SizeOf(le1)), IntPtr.Zero))
                            return null;

                        LoadedModule pme = new LoadedModule();
                        pme.ModulePath = s3;
                        pre.ListProcessModule.Add(pme);
                    }

                    return s;
                }
            }
            finally
            {
                CloseHandle(handle);
            }
        }

        private const int PROCESS_QUERY_INFORMATION = 0x400;
        private const int PROCESS_VM_READ = 0x10;

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_BASIC_INFORMATION
        {
            public IntPtr Reserved1;
            public IntPtr PebBaseAddress;
            public IntPtr Reserved2_0;
            public IntPtr Reserved2_1;
            public IntPtr UniqueProcessId;
            public IntPtr Reserved3;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UNICODE_STRING
        {
            public short Length;
            public short MaximumLength;
            public IntPtr Buffer;
        }

        // for 32-bit process in a 64-bit OS only
        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_BASIC_INFORMATION_WOW64
        {
            public long Reserved1;
            public long PebBaseAddress;
            public long Reserved2_0;
            public long Reserved2_1;
            public long UniqueProcessId;
            public long Reserved3;
        }

        // for 32-bit process
        [StructLayout(LayoutKind.Sequential)]
        private struct UNICODE_STRING_WOW64
        {
            public short Length;
            public short MaximumLength;
            public long Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct UNICODE_STRING_32
        {
            public short Length;
            public short MaximumLength;
            public int Buffer;
        }

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr ProcessHandle, int ProcessInformationClass, ref PROCESS_BASIC_INFORMATION ProcessInformation, int ProcessInformationLength, IntPtr ReturnLength);

        //ProcessWow64Information, // q: ULONG_PTR
        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr ProcessHandle, int ProcessInformationClass, ref IntPtr ProcessInformation, int ProcessInformationLength, IntPtr ReturnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, ref IntPtr lpBuffer, IntPtr dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, ref UNICODE_STRING lpBuffer, IntPtr dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, ref UNICODE_STRING_32 lpBuffer, IntPtr dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [MarshalAs(UnmanagedType.LPWStr)] string lpBuffer, IntPtr dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        // for 32-bit process in a 64-bit OS only
        [DllImport("ntdll.dll")]
        private static extern int NtWow64QueryInformationProcess64(IntPtr ProcessHandle, int ProcessInformationClass, ref PROCESS_BASIC_INFORMATION_WOW64 ProcessInformation, int ProcessInformationLength, IntPtr ReturnLength);

        [DllImport("ntdll.dll")]
        private static extern int NtWow64ReadVirtualMemory64(IntPtr hProcess, long lpBaseAddress, ref long lpBuffer, long dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("ntdll.dll")]
        private static extern int NtWow64ReadVirtualMemory64(IntPtr hProcess, long lpBaseAddress, ref UNICODE_STRING_WOW64 lpBuffer, long dwSize, IntPtr lpNumberOfBytesRead);

        [DllImport("ntdll.dll")]
        private static extern int NtWow64ReadVirtualMemory64(IntPtr hProcess, long lpBaseAddress, [MarshalAs(UnmanagedType.LPWStr)] string lpBuffer, long dwSize, IntPtr lpNumberOfBytesRead);

        //inner enum used only internally
        [Flags]
        private enum SnapshotFlags : uint
        {
            HeapList = 0x00000001,
            Process = 0x00000002,
            Thread = 0x00000004,
            Module = 0x00000008,
            Module32 = 0x00000010,
            Inherit = 0x80000000,
            All = 0x0000001F,
            NoHeaps = 0x40000000
        }

        //inner struct used only internally
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct PROCESSENTRY32
        {
            const int MAX_PATH = 260;
            internal UInt32 dwSize;
            internal UInt32 cntUsage;
            internal UInt32 th32ProcessID;
            internal IntPtr th32DefaultHeapID;
            internal UInt32 th32ModuleID;
            internal UInt32 cntThreads;
            internal UInt32 th32ParentProcessID;
            internal Int32 pcPriClassBase;
            internal UInt32 dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            internal string szExeFile;
        }

        [DllImport("kernel32", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr CreateToolhelp32Snapshot([In]UInt32 dwFlags, [In]UInt32 th32ProcessID);

        [DllImport("kernel32", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern bool Process32First([In]IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern bool Process32Next([In]IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        public static List<RunningProcess> StartAudit()
        {

            IntPtr handleToSnapshot = IntPtr.Zero;
            List<RunningProcess> prelem = new List<RunningProcess>();
            Dictionary<string, Forensics.CryptInfo> lstCertInfo = new Dictionary<string, Forensics.CryptInfo>();
            try
            {
                PROCESSENTRY32 procEntry = new PROCESSENTRY32();
                procEntry.dwSize = (UInt32)Marshal.SizeOf(typeof(PROCESSENTRY32));
                handleToSnapshot = CreateToolhelp32Snapshot((uint)SnapshotFlags.Process, 0);
                if (Process32First(handleToSnapshot, ref procEntry))
                {
                    do
                    {
                        RunningProcess pre = new RunningProcess();
                        pre.ListProcessModule = new List<LoadedModule>();

                        pre.PID = procEntry.th32ProcessID;
                        pre.ProcessName = procEntry.szExeFile;
                        pre.PPID = procEntry.th32ParentProcessID;
                        pre.ParentName = GetMainModuleFilepath((int)pre.PPID);
                        string s1 = winaudits.ProcessList.GetProcessParametersString((int)procEntry.th32ProcessID,
                                 PEB_OFFSET.CommandLine, pre);

                        if (!string.IsNullOrEmpty(pre.Path))
                        {
                            Forensics.CryptInfo ci;
                            if (lstCertInfo.ContainsKey(pre.Path))
                            {
                                ci = lstCertInfo[pre.Path];
                            }
                            else
                            {
                                ci = Forensics.SigVerify.CheckSignatureForFile(pre.Path);
                                ci.MD5 = Forensics.ProxyMD5.ComputeFileMD5(pre.Path);
                                lstCertInfo.Add(pre.Path, ci);
                            }
                            pre.MD5 = ci.MD5;
                            pre.IsSigned = ci.IsSigned;
                            pre.IsVerified = ci.IsVerified;
                            pre.CertSubject = ci.Subject;
                            pre.CA = ci.CA;
                            pre.SignatureString = ci.Signature;
                        }

                        foreach (var modu in pre.ListProcessModule)
                        {
                            modu.ProcessId = (int)pre.PID;
                            if (!string.IsNullOrEmpty(modu.ModulePath))
                            {
                                modu.ModuleName = Path.GetFileName(modu.ModulePath);
                            }
                            Forensics.CryptInfo ci;
                            if (lstCertInfo.ContainsKey(modu.ModulePath))
                            {
                                ci = lstCertInfo[modu.ModulePath];
                            }
                            else
                            {
                                ci = Forensics.SigVerify.CheckSignatureForFile(modu.ModulePath);
                                ci.MD5 = Forensics.ProxyMD5.ComputeFileMD5(modu.ModulePath);
                                lstCertInfo.Add(modu.ModulePath, ci);
                            }
                            modu.MD5 = ci.MD5;
                            modu.IsSigned = ci.IsSigned;
                            modu.IsVerified = ci.IsVerified;
                            modu.CertSubject = ci.Subject;
                            modu.CA = ci.CA;
                            modu.SignatureString = ci.Signature;
                        }
                        prelem.Add(pre);
                    } while (Process32Next(handleToSnapshot, ref procEntry));
                }
            }
            catch (Exception)
            {
                return prelem;
            }
            finally
            {
                // Must clean up the snapshot object!
                CloseHandle(handleToSnapshot);
            }
            return prelem;
        }

        private static string GetMainModuleFilepath(int processId)
        {
            if (processId == 0 || processId == 4)
            {
                return null;
            }
            try
            {
                string wmiQueryString = "SELECT ProcessId, ExecutablePath FROM Win32_Process WHERE ProcessId = " + processId;
                using (var searcher = new ManagementObjectSearcher(wmiQueryString))
                {
                    using (var results = searcher.Get())
                    {
                        ManagementObject mo = results.Cast<ManagementObject>().FirstOrDefault();
                        if (mo != null)
                        {
                            return (string)mo["ExecutablePath"];
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return string.Empty;
        }
    }
}
