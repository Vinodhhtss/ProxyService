using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace HTTPProxyServer
{
    internal static class TcpClientIDNative
    {
        public static class NativeMethods
        {
            [DllImport("iphlpapi.dll", ExactSpelling = true, SetLastError = true)]
            public static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref uint dwTcpTableLength, [MarshalAs(UnmanagedType.Bool)] bool sort, uint ipVersion, TcpClientIDNative.TcpTableType tcpTableType, uint reserved);
        }

        public enum TcpTableType
        {
            BasicListener,
            BasicConnections,
            BasicAll,
            OwnerPidListener,
            OwnerPidConnections,
            OwnerPidAll,
            OwnerModuleListener,
            OwnerModuleConnections,
            OwnerModuleAll
        }
        private const int AF_INET = 2;
        private const int AF_INET6 = 23;
        private const int ERROR_INSUFFICIENT_BUFFER = 122;
        private const int NO_ERROR = 0;

        internal static int LocalPortToPIDMap(int iPort)
        {
            return TcpClientIDNative.GetPIDForPort(iPort);
        }

        private static int GetPIDForConn(int iTargetPort, uint iAddressType, TcpClientIDNative.TcpTableType whichTable)
        {
            IntPtr intPtr = IntPtr.Zero;
            
            uint num = 32768u;
            try
            {
                intPtr = Marshal.AllocHGlobal(32768);
                uint extendedTcpTable = TcpClientIDNative.NativeMethods.GetExtendedTcpTable(intPtr, ref num, false, iAddressType, whichTable, 0u);
                while (122u == extendedTcpTable)
                {
                    Marshal.FreeHGlobal(intPtr);
                    num += 2048u;
                    intPtr = Marshal.AllocHGlobal((int)num);
                    extendedTcpTable = TcpClientIDNative.NativeMethods.GetExtendedTcpTable(intPtr, ref num, false, iAddressType, whichTable, 0u);
                }
                if (extendedTcpTable != 0u)
                {
                    int result = 0;
                    return result;
                }
                int num2;
                int ofs;
                int num3;
                if (iAddressType == 2u)
                {
                    num2 = 12;
                    ofs = 12;
                    num3 = 24;
                }
                else
                {
                    num2 = 24;
                    ofs = 32;
                    num3 = 56;
                }
                int num4 = ((iTargetPort & 255) << 8) + ((iTargetPort & 65280) >> 8);
                int num5 = Marshal.ReadInt32(intPtr);
                if (num5 == 0)
                {
                    int result = 0;
                    return result;
                }
                IntPtr intPtr2 = (IntPtr)((long)intPtr + (long)num2);
                for (int i = 0; i < num5; i++)
                {
                    if (num4 == Marshal.ReadInt32(intPtr2))
                    {
                        int result = Marshal.ReadInt32(intPtr2, ofs);
                        return result;
                    }
                    intPtr2 = (IntPtr)((long)intPtr2 + (long)num3);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(intPtr);
            }
            return 0;
        }

        private static int GetPIDForPort(int iTargetPort)
        {
            try
            {
                int num = TcpClientIDNative.GetPIDForConn(iTargetPort, 2u, TcpClientIDNative.TcpTableType.OwnerPidConnections);
                int result;
                if (num > 0)
                {
                    result = num;
                    return result;
                }
                result = TcpClientIDNative.GetPIDForConn(iTargetPort, 23u, TcpClientIDNative.TcpTableType.OwnerPidConnections);
                return result;
            }
            catch (Exception ex)
            {
                ////TCPClientProcessor.Proxylog.Logger.Error(ex);
            }
            return 0;
        }

        internal static string GetListeningProcess(int iPort)
        {
            string result;
            try
            {
                int num = TcpClientIDNative.GetPIDForConn(iPort, 2u, TcpClientIDNative.TcpTableType.OwnerPidListener);
                if (num < 1)
                {
                    num = TcpClientIDNative.GetPIDForConn(iPort, 23u, TcpClientIDNative.TcpTableType.OwnerPidListener);
                }
                if (num < 1)
                {
                    result = string.Empty;
                }
                else
                {
                    string text = Process.GetProcessById(num).ProcessName.ToLower();
                    if (string.IsNullOrEmpty(text))
                    {
                        text = "unknown";
                    }
                    result = text + ":" + num.ToString();
                }
            }
            catch (Exception ex)
            {
                ////TCPClientProcessor.Proxylog.Logger.Error(ex);
                result = string.Empty;
            }
            return result;
        }
    }
}
