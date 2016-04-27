using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;

namespace winaudits
{
    public class ARP
    {
        [JsonProperty("interface")]
        public string Interface { get; set; }
        [JsonProperty("physicaladdress")]
        public string PhysicalAddress { get; set; }
        [JsonProperty("ip4address")]
        public string IP4Address { get; set; }
        [JsonProperty("cachetype")]
        public string CacheType { get; set; }
    }

    public class ARPAuditor
    {
        // Define the MIB_IPNETROW structure.
        [StructLayout(LayoutKind.Sequential)]
        struct MIB_IPNETROW
        {
            [MarshalAs(UnmanagedType.U4)]
            public int dwIndex;
            [MarshalAs(UnmanagedType.U4)]
            public int dwPhysAddrLen;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac0;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac1;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac2;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac3;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac4;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac5;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac6;
            [MarshalAs(UnmanagedType.U1)]
            public byte mac7;
            [MarshalAs(UnmanagedType.U4)]
            public int dwAddr;
            [MarshalAs(UnmanagedType.U4)]
            public int dwType;
        }

        // Declare the GetIpNetTable function.
        [DllImport("IpHlpApi.dll")]
        [return: MarshalAs(UnmanagedType.U4)]
        static extern int GetIpNetTable(
           IntPtr pIpNetTable,
           [MarshalAs(UnmanagedType.U4)]
         ref int pdwSize,
           bool bOrder);

        [DllImport("IpHlpApi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern int FreeMibTable(IntPtr plpNetTable);

        // The insufficient buffer error.
        const int ERROR_INSUFFICIENT_BUFFER = 122;

        enum dwTypes
        {
            Static = 4,
            Dynamic = 3,
            Invalid = 2,
            Other = 1,
        }

        public static List<ARP> StartAudit()
        {
            List<ARP> lstArp = new List<ARP>();
            try
            {
                // The number of bytes needed.
                int bytesNeeded = 0;

                // The result from the API call.
                int result = GetIpNetTable(IntPtr.Zero, ref bytesNeeded, false);

                // Call the function, expecting an insufficient buffer.
                if (result != ERROR_INSUFFICIENT_BUFFER)
                {
                    // Throw an exception.
                    return lstArp;
                }

                // Allocate the memory, do it in a try/finally block, to ensure
                // that it is released.
                IntPtr buffer = IntPtr.Zero;

                // Try/finally.
                try
                {
                    // Allocate the memory.
                    buffer = Marshal.AllocCoTaskMem(bytesNeeded);

                    // Make the call again. If it did not succeed, then
                    // raise an error.
                    result = GetIpNetTable(buffer, ref bytesNeeded, false);

                    // If the result is not 0 (no error), then throw an exception.
                    if (result != 0)
                    {
                        // Throw an exception.
                        return lstArp;
                    }

                    // Now we have the buffer, we have to marshal it. We can read
                    // the first 4 bytes to get the length of the buffer.
                    int entries = Marshal.ReadInt32(buffer);

                    // Increment the memory pointer by the size of the int.
                    IntPtr currentBuffer = new IntPtr(buffer.ToInt64() +
                       Marshal.SizeOf(typeof(int)));

                    // Allocate an array of entries.
                    MIB_IPNETROW[] table = new MIB_IPNETROW[entries];

                    // Cycle through the entries.
                    for (int index = 0; index < entries; index++)
                    {
                        // Call PtrToStructure, getting the structure information.
                        table[index] = (MIB_IPNETROW)Marshal.PtrToStructure(new
                           IntPtr(currentBuffer.ToInt64() + (index *
                           Marshal.SizeOf(typeof(MIB_IPNETROW)))), typeof(MIB_IPNETROW));
                    }
                    NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                    Dictionary<int, string> adapterNameAndIndex = PrintInterfaceIndex();
                    for (int index = 0; index < entries; index++)
                    {
                        ARP oarp = new ARP();

                        MIB_IPNETROW row = table[index];
                        if (adapterNameAndIndex.ContainsKey(row.dwIndex))
                        {
                            oarp.Interface = adapterNameAndIndex[row.dwIndex];
                        }
                        else
                        {
                            oarp.Interface = row.dwIndex.ToString();
                        }
                        IPAddress ip = new IPAddress(BitConverter.GetBytes(row.dwAddr));
                        oarp.IP4Address = ip.ToString();
                        StringBuilder mac = new StringBuilder();
                        mac.Append(row.mac0.ToString("X2"));
                        mac.Append(row.mac0.ToString("-"));
                        mac.Append(row.mac1.ToString("X2"));
                        mac.Append(row.mac1.ToString("-"));
                        mac.Append(row.mac2.ToString("X2"));
                        mac.Append(row.mac2.ToString("-"));
                        mac.Append(row.mac3.ToString("X2"));
                        mac.Append(row.mac3.ToString("-"));
                        mac.Append(row.mac4.ToString("X2"));
                        oarp.PhysicalAddress = mac.ToString();

                        oarp.CacheType = ((dwTypes)row.dwType).ToString();
                        lstArp.Add(oarp);
                    }
                }
                finally
                {
                    // Release the memory.
                    FreeMibTable(buffer);
                }
            }
            catch (Exception)
            {
                throw;
                // //logger.Error(ex);
            }
            return lstArp;
        }

        static Dictionary<int, string> PrintInterfaceIndex()
        {
            Dictionary<int, string> adapterNames = new Dictionary<int, string>();
            try
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

                foreach (NetworkInterface adapter in nics)
                {
                    IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
                    IPv4InterfaceProperties p = adapterProperties.GetIPv4Properties();
                    if (p == null)
                    {
                        continue;
                    }

                    if (!adapterNames.ContainsKey(p.Index))
                    {
                        adapterNames.Add(p.Index, adapter.Name);
                    }
                }
            }
            catch (Exception)
            {
                
            }
            return adapterNames;
        }
    }
}
