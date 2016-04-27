using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Forensics
{
    internal class NativeMethods
    {
        internal const int ERROR_HANDLE_EOF = 38;

        //--> Privilege constants....
        internal const UInt32 SE_PRIVILEGE_ENABLED = 0x00000002;
        internal const string SE_BACKUP_NAME = "SeBackupPrivilege";
        internal const string SE_RESTORE_NAME = "SeRestorePrivilege";
        internal const string SE_SECURITY_NAME = "SeSecurityPrivilege";
        internal const string SE_CHANGE_NOTIFY_NAME = "SeChangeNotifyPrivilege";
        internal const string SE_CREATE_SYMBOLIC_LINK_NAME = "SeCreateSymbolicLinkPrivilege";
        internal const string SE_CREATE_PERMANENT_NAME = "SeCreatePermanentPrivilege";
        internal const string SE_SYSTEM_ENVIRONMENT_NAME = "SeSystemEnvironmentPrivilege";
        internal const string SE_SYSTEMTIME_NAME = "SeSystemtimePrivilege";
        internal const string SE_TIME_ZONE_NAME = "SeTimeZonePrivilege";
        internal const string SE_TCB_NAME = "SeTcbPrivilege";
        internal const string SE_MANAGE_VOLUME_NAME = "SeManageVolumePrivilege";
        internal const string SE_TAKE_OWNERSHIP_NAME = "SeTakeOwnershipPrivilege";

        //--> For starting a process in session 1 from session 0...
        internal const int TOKEN_DUPLICATE = 0x0002;
        internal const uint MAXIMUM_ALLOWED = 0x2000000;
        internal const int CREATE_NEW_CONSOLE = 0x00000010;
        internal const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        internal const int TOKEN_QUERY = 0x00000008;

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);
        [DllImport("kernel32.dll")]
        internal static extern IntPtr GetCurrentProcess();
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out LUID lpLuid);
        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, [MarshalAs(UnmanagedType.Bool)]bool DisableAllPrivileges, ref TOKEN_PRIVILEGES NewState, Int32 BufferLength, IntPtr PreviousState, IntPtr ReturnLength);
    
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseHandle(IntPtr hObject);



        [StructLayout(LayoutKind.Sequential)]
        internal struct LUID
        {
            public UInt32 LowPart;
            public Int32 HighPart;
        }


        [StructLayout(LayoutKind.Sequential)]
        internal struct LUID_AND_ATTRIBUTES
        {
            public LUID Luid;
            public UInt32 Attributes;
        }


        internal struct TOKEN_PRIVILEGES
        {
            public UInt32 PrivilegeCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]      // !! think we only need one
            public LUID_AND_ATTRIBUTES[] Privileges;
        }

    }

    public static class Privileges
    {
        private static int asserted = 0;
        private static bool hasBackupPrivileges = false;

        public static bool HasBackupAndRestorePrivileges
        {
            get { return AssertPriveleges(); }
        }

        /// <remarks>
        /// First time this method is called, it attempts to set backup privileges for the current process.
        /// Subsequently, it returns the results of that first call.
        /// </remarks>
        private static bool AssertPriveleges()
        {
           bool success = false;
           var wasAsserted = Interlocked.CompareExchange( ref asserted, 1, 0 );
           if ( wasAsserted == 0 )  // first time here?  come on in!
           {
               success =
                  AssertPrivelege(NativeMethods.SE_BACKUP_NAME);
                  AssertPrivelege( NativeMethods.SE_RESTORE_NAME );

                  hasBackupPrivileges = success;

           }
           return hasBackupPrivileges;
        }


        private static bool AssertPrivelege(string privelege)
        {
            IntPtr token;
            var tokenPrivileges = new NativeMethods.TOKEN_PRIVILEGES();
            tokenPrivileges.Privileges = new NativeMethods.LUID_AND_ATTRIBUTES[1];

            var success =
              NativeMethods.OpenProcessToken(NativeMethods.GetCurrentProcess(), NativeMethods.TOKEN_ADJUST_PRIVILEGES, out token)
              &&
              NativeMethods.LookupPrivilegeValue(null, privelege, out tokenPrivileges.Privileges[0].Luid);

            try
            {
                if (success)
                {
                    tokenPrivileges.PrivilegeCount = 1;
                    tokenPrivileges.Privileges[0].Attributes = NativeMethods.SE_PRIVILEGE_ENABLED;
                    success =
                      NativeMethods.AdjustTokenPrivileges(token, false, ref tokenPrivileges, Marshal.SizeOf(tokenPrivileges), IntPtr.Zero, IntPtr.Zero)
                      &&
                      (Marshal.GetLastWin32Error() == 0);
                }

                if (!success)
                {
                    Console.WriteLine("Could not assert privilege: " + privelege);
                }
            }
            finally
            {
                NativeMethods.CloseHandle(token);
            }

            return success;
        }
    }
}
