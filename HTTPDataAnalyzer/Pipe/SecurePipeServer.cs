using System;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;

namespace HTTPDataAnalyzer
{
    public class SecurePipeServer : IPipeHandler
    {
        public PipeServer m_srv;
        private Int32 m_count;

        public String m_pipename;
        public int m_instcount;

        public SecurePipeServer(string pipename, int instance)
        {
            m_pipename = pipename;
            m_instcount = instance;
        }

        public void Start(int pipeServerType)
        {
            m_srv = new PipeServer(m_pipename, this, m_instcount, pipeServerType);
        }

        public void Stop()
        {
            m_srv.PipeServerStop();
        }

        public void OnConnect(PipeStream pipe, out Object state)
        {
            Int32 count = Interlocked.Increment(ref m_count);
            Console.WriteLine("Connected : " + count);
            state = count;
        }

        public void OnDisconnect(PipeStream pipe, Object state)
        {
            Console.WriteLine("Disconnected :" + (Int32)state);
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BASE_PIPE_HEADER
    {
        [MarshalAs(UnmanagedType.U2)]
        public UInt16 msgType;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16 msgLen;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct DNS_PIPE_PACKET
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string dnsString;

        [MarshalAs(UnmanagedType.U4)]
        public UInt32 pId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string filePath;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string procName;

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct DNS_FULL_PACKET
    {
        public BASE_PIPE_HEADER PipeHeader;
        public DNS_PIPE_PACKET dnsPacket;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct FileCreation_PIPE_PACKET
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
        public string filePath;

        [MarshalAs(UnmanagedType.U2)]
        public UInt16   fileType;

        [MarshalAs(UnmanagedType.I1)]
        public bool isSuccess;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string md5;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string signature;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string version;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    public struct FileCreation_FULL_PACKET
    {
        public BASE_PIPE_HEADER PipeHeader;
        public FileCreation_PIPE_PACKET filePacket;
    }

    public static class PipeStructConverter
    {
        public static bool ByteArrayToDNS(byte[] bytearray, ref object obj)
        {
            try
            {
                int len = Marshal.SizeOf(obj);
                IntPtr i = Marshal.AllocHGlobal(len);
                Marshal.Copy(bytearray, 0, i, len);
                obj = Marshal.PtrToStructure(i, obj.GetType());
                Marshal.FreeHGlobal(i);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
