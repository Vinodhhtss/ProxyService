using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace AddRegisterEntriesInstaller
{
    public class Dialup
    {
        #region <Fields>

        private static int rasConnectionsAmount;
        #endregion

        [DllImport("wininet.dll", CharSet = CharSet.Auto)]
        static extern bool InternetGetConnectedState(
            ref int lpdwFlags,
            int dwReserved);

        const int MAX_PATH = 260;
        const int RAS_MaxDeviceType = 16;
        const int RAS_MaxPhoneNumber = 128;
        const int RAS_MaxEntryName = 256;
        const int RAS_MaxDeviceName = 128;

        const int RAS_Connected = 0x2000;

        const int ERROR_BUFFER_TOO_SMALL = 603;
        const int ERROR_SUCCESS = 0;


        [DllImport("rasapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern uint RasHangUp(IntPtr hRasConn);

        [DllImport("RASAPI32", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int RasGetConnectStatus(IntPtr hrasconn, ref RASCONNSTATUS lprasconnstatus);

        [DllImport("rasapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int RasEnumConnections(
            [In, Out] RASCONN[] rasconn,
            [In, Out] ref int cb,
            [Out] out int connections);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct RASCONN
        {
            public int dwSize;
            public IntPtr hrasconn;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxEntryName + 1)]
            public string szEntryName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxDeviceType + 1)]
            public string szDeviceType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxDeviceName + 1)]
            public string szDeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string szPhonebook;
            public int dwSubEntry;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct RASCONNSTATUS
        {
            public int dwSize;
            public int rasconnstate;
            public int dwError;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxDeviceType + 1)]
            public string szDeviceType;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxDeviceName + 1)]
            public string szDeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = RAS_MaxPhoneNumber + 1)]
            public string szPhoneNumber;
        }



        public static RASCONN[] GetRasConnections()
        {
            RASCONN[] connections = new RASCONN[1];
            connections[0].dwSize = Marshal.SizeOf(typeof(RASCONN));
            //Get entries count
            int connectionsCount = 0;
            int cb = Marshal.SizeOf(typeof(RASCONN));
            int nRet = RasEnumConnections(connections, ref cb, out connectionsCount);
            if (nRet != ERROR_SUCCESS && nRet != ERROR_BUFFER_TOO_SMALL)
            {
               // throw new Win32Exception(nRet);
            }
            if (connectionsCount == 0)
            {
                return new RASCONN[0];
            }
            // create array with specified entries number
            connections = new RASCONN[connectionsCount];
            for (int i = 0; i < connections.Length; i++)
            {
                connections[i].dwSize = Marshal.SizeOf(typeof(RASCONN));
            }
            nRet = RasEnumConnections(connections, ref cb, out connectionsCount);
            if (nRet != ERROR_SUCCESS)
            {
              //  throw new Win32Exception((int)nRet);
            }

            return connections;
        }

    }
}
