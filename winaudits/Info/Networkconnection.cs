using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Runtime.InteropServices;

namespace winaudits
{
    public class Networkconnection
    {
        [JsonProperty("processname")]
        public string ProcessName { get; set; }
        [JsonProperty("pid")]
        public uint PID { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("created")]
        public string Created { get; set; }
        [JsonProperty("localip")]
        public string LocalIP { get; set; }
        [JsonProperty("localport")]
        public string LocalPort { get; set; }
        [JsonProperty("remoteip")]
        public string RemoteIP { get; set; }
        [JsonProperty("remoteport")]
        public string RemotePort { get; set; }
        [JsonProperty("protocol")]
        public string Protocol { get; set; }
    }

    public static class NetworkAuditor
    {
        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, TCP_TABLE_CLASS tblClass, int reserved);

        [DllImport("iphlpapi.dll", SetLastError = true)]
        private  static extern uint GetExtendedUdpTable(IntPtr pUdpTable, ref int dwOutBufLen, bool sort, int ipVersion, UDP_TABLE_CLASS tblClass, int reserved);

        [StructLayout(LayoutKind.Sequential)]
        private struct MIB_TCPROW_OWNER_PID
        {
            public uint dwState;
            public uint dwLocalAddr;
            public uint dwLocalPort;
            public uint dwRemoteAddr;
            public uint dwRemotePort;
            public uint dwOwningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MIB_UDPROW_OWNER_PID
        {
            public uint dwLocalAddr;
            public uint dwLocalPort;
            public uint dwOwningPid;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MIB_TCPTABLE_OWNER_PID
        {
            public uint dwNumEntries;
            MIB_TCPROW_OWNER_PID table;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MIB_UDPTABLE_OWNER_PID
        {
            public uint dwNumEntries;
            MIB_UDPROW_OWNER_PID table;
        }

        enum TCP_TABLE_CLASS
        {
            TCP_TABLE_BASIC_LISTENER,
            TCP_TABLE_BASIC_CONNECTIONS,
            TCP_TABLE_BASIC_ALL,
            TCP_TABLE_OWNER_PID_LISTENER,
            TCP_TABLE_OWNER_PID_CONNECTIONS,
            TCP_TABLE_OWNER_PID_ALL,
            TCP_TABLE_OWNER_MODULE_LISTENER,
            TCP_TABLE_OWNER_MODULE_CONNECTIONS,
            TCP_TABLE_OWNER_MODULE_ALL
        }

        enum UDP_TABLE_CLASS
        {
            UDP_TABLE_BASIC,
            UDP_TABLE_OWNER_PID,
            UDP_OWNER_MODULE
        }

        [DllImport("iphlpapi.dll", SetLastError = true)]
        static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, TCP_TABLE_CLASS tblClass, uint reserved = 0);

        public static List<Networkconnection> StartAudit()
        {
            List<Networkconnection> cl = new List<Networkconnection>();
            try
            {
                Connection[] ctcp = GetTCP();

                if (ctcp != null && ctcp.Length > 0)
                {
                    for (int i = 0; i < ctcp.Length; i++)
                    {
                        Networkconnection connitem = new Networkconnection();
                        connitem.Protocol = ctcp[i].Protocol;
                        connitem.LocalPort = ctcp[i].LocalPort.ToString();
                        connitem.LocalIP = ctcp[i].LocalIP.ToString();
                        connitem.State = ctcp[i].State;
                        connitem.RemoteIP = ctcp[i].RemoteIP.ToString();
                        connitem.RemotePort = ctcp[i].RemotePort.ToString();
                        connitem.Path = GetMainModuleFilepath(ctcp[i].PID);
                        connitem.ProcessName = ctcp[i].PIDName;
                        connitem.PID = (uint)ctcp[i].PID;
                        cl.Add(connitem);
                    }
                }

                Connection[] cudp = GetUDP();

                if (cudp != null && cudp.Length > 0)
                {
                    for (int i = 0; i < cudp.Length; i++)
                    {
                        Networkconnection connitem = new Networkconnection();
                        connitem.Protocol = cudp[i].Protocol;
                        connitem.LocalPort = cudp[i].LocalPort.ToString();
                        connitem.LocalIP = cudp[i].LocalIP.ToString();
                        connitem.State = cudp[i].State;
                        connitem.Path = GetMainModuleFilepath(cudp[i].PID);
                        connitem.ProcessName = cudp[i].PIDName;
                        connitem.PID = (uint)cudp[i].PID;
                        cl.Add(connitem);
                    }
                }
                return cl;
            }
            catch (Exception)
            {

            }
            return null;
        }

        private static Connection[] GetTCP()
        {

            MIB_TCPROW_OWNER_PID[] tTable;
            int AF_INET = 2;
            int buffSize = 0;

            uint ret = GetExtendedTcpTable(IntPtr.Zero, ref buffSize, true, AF_INET, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);
            IntPtr buffTable = Marshal.AllocHGlobal(buffSize);

            try
            {
                ret = GetExtendedTcpTable(buffTable, ref buffSize, true, AF_INET, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);
                if (ret != 0)
                {
                    Connection[] con = new Connection[0];
                    return con;
                }

                MIB_TCPTABLE_OWNER_PID tab = (MIB_TCPTABLE_OWNER_PID)Marshal.PtrToStructure(buffTable, typeof(MIB_TCPTABLE_OWNER_PID));
                IntPtr rowPtr = (IntPtr)((long)buffTable + Marshal.SizeOf(tab.dwNumEntries));
                tTable = new MIB_TCPROW_OWNER_PID[tab.dwNumEntries];

                for (int i = 0; i < tab.dwNumEntries; i++)
                {
                    MIB_TCPROW_OWNER_PID tcpRow = (MIB_TCPROW_OWNER_PID)Marshal.PtrToStructure(rowPtr, typeof(MIB_TCPROW_OWNER_PID));
                    tTable[i] = tcpRow;
                    rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(tcpRow));   // next entry
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffTable);
            }

            Connection[] cons = new Connection[tTable.Length];

            for (int i = 0; i < tTable.Length; i++)
            {
                IPAddress localip = new IPAddress(BitConverter.GetBytes(tTable[i].dwLocalAddr));
                IPAddress remoteip = new IPAddress(BitConverter.GetBytes(tTable[i].dwRemoteAddr));
                byte[] barray = BitConverter.GetBytes(tTable[i].dwLocalPort);
                int localport = (barray[0] * 256) + barray[1];
                barray = BitConverter.GetBytes(tTable[i].dwRemotePort);
                int remoteport = (barray[0] * 256) + barray[1];
                string state;
                switch (tTable[i].dwState)
                {
                    case 1:
                        state = "Closed";
                        break;
                    case 2:
                        state = "LISTENING";
                        break;
                    case 3:
                        state = "SYN SENT";
                        break;
                    case 4:
                        state = "SYN RECEIVED";
                        break;
                    case 5:
                        state = "ESTABLISHED";
                        break;
                    case 6:
                        state = "FINSIHED 1";
                        break;
                    case 7:
                        state = "FINISHED 2";
                        break;
                    case 8:
                        state = "CLOSE WAIT";
                        break;
                    case 9:
                        state = "CLOSING";
                        break;
                    case 10:
                        state = "LAST ACKNOWLEDGE";
                        break;
                    case 11:
                        state = "TIME WAIT";
                        break;
                    case 12:
                        state = "DELETE TCB";
                        break;
                    default:
                        state = "UNKNOWN";
                        break;
                }
                Connection tmp = new Connection(localip, localport, remoteip, remoteport, (int)tTable[i].dwOwningPid, state);
                cons[i] = (tmp);
            }
            return cons;
        }


        private static Connection[] GetUDP()
        {
            MIB_UDPROW_OWNER_PID[] tTable;
            int AF_INET = 2; // IP_v4
            int buffSize = 0;

            uint ret = GetExtendedUdpTable(IntPtr.Zero, ref buffSize, true, AF_INET, UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID, 0);
            IntPtr buffTable = Marshal.AllocHGlobal(buffSize);

            try
            {
                ret = GetExtendedUdpTable(buffTable, ref buffSize, true, AF_INET, UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID, 0);
                if (ret != 0)
                {//none found
                    Connection[] con = new Connection[0];
                    return con;
                }
                MIB_UDPTABLE_OWNER_PID tab = (MIB_UDPTABLE_OWNER_PID)Marshal.PtrToStructure(buffTable, typeof(MIB_UDPTABLE_OWNER_PID));
                IntPtr rowPtr = (IntPtr)((long)buffTable + Marshal.SizeOf(tab.dwNumEntries));
                tTable = new MIB_UDPROW_OWNER_PID[tab.dwNumEntries];

                for (int i = 0; i < tab.dwNumEntries; i++)
                {
                    MIB_UDPROW_OWNER_PID udprow = (MIB_UDPROW_OWNER_PID)Marshal.PtrToStructure(rowPtr, typeof(MIB_UDPROW_OWNER_PID));
                    tTable[i] = udprow;
                    rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(udprow));
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffTable);
            }

            Connection[] cons = new Connection[tTable.Length];

            for (int i = 0; i < tTable.Length; i++)
            {
                IPAddress localip = new IPAddress(BitConverter.GetBytes(tTable[i].dwLocalAddr));
                byte[] barray = BitConverter.GetBytes(tTable[i].dwLocalPort);
                int localport = (barray[0] * 256) + barray[1];
                Connection tmp = new Connection(localip, localport, (int)tTable[i].dwOwningPid);
                cons[i] = tmp;
            }
            return cons;
        }

        private static string GetMainModuleFilepath(int processId)
        {
            if(processId == 0 || processId == 4)
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

    internal class Connection
    {
        private IPAddress _localip, _remoteip;
        private int _localport, _remoteport, _pid;
        private string _state, _remotehost, _proto;

        public Connection(IPAddress Local, int LocalPort, IPAddress Remote, int RemotePort, int PID, string State)
        {
            _proto = "TCP";
            _localip = Local;
            _remoteip = Remote;
            _localport = LocalPort;
            _remoteport = RemotePort;
            _pid = PID;
            _state = State;
        }

        public Connection(IPAddress Local, int LocalPort, int PID)
        {
            _proto = "UDP";
            _localip = Local;
            _localport = LocalPort;
            _pid = PID;
        }
        public IPAddress LocalIP { get { return _localip; } }
        public IPAddress RemoteIP { get { return _remoteip; } }
        public int LocalPort { get { return _localport; } }
        public int RemotePort { get { return _remoteport; } }
        public int PID { get { return _pid; } }
        public string State { get { return _state; } }
        public string Protocol { get { return _proto; } }

        public string RemoteHostName
        {
            get
            {
                if (_remotehost == null)
                    _remotehost = Dns.GetHostEntry(_remoteip).HostName;
                return _remotehost;
            }
        }

        public string PIDName
        {
            get
            {
                string processName = string.Empty;
                try
                {
                    processName = (System.Diagnostics.Process.GetProcessById(_pid)).ProcessName;
                }
                catch (Exception)
                {
                }
                return processName;
            }
        }
    }
}
