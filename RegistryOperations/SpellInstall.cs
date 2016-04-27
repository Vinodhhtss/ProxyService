using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

//{79633098-C0A1-4898-8B11-55D2951116FB}

namespace AddRegisterEntriesInstaller.SpellInstall
{
    public static class NspInstall
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct GUID
        {
            public int a;
            public short b;
            public short c;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] d;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WSAData
        {
            internal Int16 version;
            internal Int16 highVersion;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 257)]
            internal String description;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 129)]
            internal String systemStatus;

            internal Int16 maxSockets;
            internal Int16 maxUdpDg;
            internal IntPtr vendorInfo;
        }

        [DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Int32 WSAStartup(Int16 wVersionRequested, out WSAData wsaData);

        [DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern Int32 WSACleanup();

        [DllImport("ws2_32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int WSCInstallNameSpace(
             [MarshalAs(UnmanagedType.LPWStr)] string lpszIdentifier,
             [MarshalAs(UnmanagedType.LPWStr)] string lpszPathName,
             [MarshalAs(UnmanagedType.U4)] UInt32 id,
             [MarshalAs(UnmanagedType.U4)] UInt32 dwVersion,
             ref GUID guid);

        [DllImport("ws2_32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int WSCUnInstallNameSpace(ref GUID guid);

        [DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern Int32 WSAGetLastError();


        public static uint MakeWord(byte low, byte high)
        {
            return ((uint)high << 8) | low;
        }

        public static bool InstallNSP()
        {
            WSAData data = new WSAData();
            GUID guid = new GUID();
            guid.d = new byte[8];
            int result = 0;

            data.highVersion = 2;
            data.version = 2;

            short version = (short)MakeWord(2, 2);

            result = WSAStartup(version, out data);
            if (result == 0)
            {
                string systempath = Environment.SystemDirectory;

                string nsppath = systempath + "\\" + "spellnsp.dll";

                //79633098-C0A1-4898-8B11-55D2951116FB}
                guid.a = 0x79633098;
                guid.b = 0x4091;
                guid.c = 0x4898;
                guid.d[0] = 0x55;
                guid.d[1] = 0xD2;
                guid.d[2] = 0x95;
                guid.d[3] = 0x11;
                guid.d[4] = 0x16;
                guid.d[5] = 0xFB;

                int retval = WSCInstallNameSpace("Caching NSP", nsppath, 12, 1, ref guid);
                if (retval != 0)
                {
                    Int32 errval = WSAGetLastError();
                    Console.WriteLine(" Error : " + errval.ToString());
                }
                Console.WriteLine(data.description);
                WSACleanup();
            }
            
            return true;
        }

        public static bool UInstallNSP()
        {
            WSAData data = new WSAData();
            GUID guid = new GUID();
            guid.d = new byte[8];
            int result = 0;

            data.highVersion = 2;
            data.version = 2;

            short version = (short)MakeWord(2, 2);

            result = WSAStartup(version, out data);
            if (result == 0)
            {
                //79633098-C0A1-4898-8B11-55D2951116FB}
                guid.a = 0x79633098;
                guid.b = 0x4091;
                guid.c = 0x4898;
                guid.d[0] = 0x55;
                guid.d[1] = 0xD2;
                guid.d[2] = 0x95;
                guid.d[3] = 0x11;
                guid.d[4] = 0x16;
                guid.d[5] = 0xFB;

                int retval = WSCUnInstallNameSpace(ref guid);
                if (retval != 0)
                {
                    Int32 errval = WSAGetLastError();
                    Console.WriteLine(" Error : " + errval.ToString());
                }

                WSACleanup();
            }

            return true;
        }
    }
}



