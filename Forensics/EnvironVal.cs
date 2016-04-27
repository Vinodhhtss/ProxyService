using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Runtime.InteropServices;

namespace Forensics
{
    public class EnvironVal
    {
        private String[] suslocations;



        [DllImport("kernel32.dll")]
        static extern bool ProcessIdToSessionId(uint dwProcessId, out uint pSessionId);

        [DllImport("Wtsapi32.dll")]
        public static extern bool WTSQuerySessionInformation(
            System.IntPtr pServer,
            int iSessionID,
            WTS_INFO_CLASS oInfoClass,
            out System.IntPtr pBuffer,
            out uint iBytesReturned);

        public enum WTS_INFO_CLASS
        {
            WTSInitialProgram,
            WTSApplicationName,
            WTSWorkingDirectory,
            WTSOEMId,
            WTSSessionId,
            WTSUserName,
            WTSWinStationName,
            WTSDomainName,
            WTSConnectState,
            WTSClientBuildNumber,
            WTSClientName,
            WTSClientDirectory,
            WTSClientProductId,
            WTSClientHardwareId,
            WTSClientAddress,
            WTSClientDisplay,
            WTSClientProtocolType,
            WTSIdleTime,
            WTSLogonTime,
            WTSIncomingBytes,
            WTSOutgoingBytes,
            WTSIncomingFrames,
            WTSOutgoingFrames,
            WTSClientInfo,
            WTSSessionInfo,
            WTSConfigInfo,
            WTSValidationInfo,
            WTSSessionAddressV4,
            WTSIsRemoteSession
        }

        public String GetUserForPid(UInt32 pid)
        {
            String username = String.Empty;
            uint pSessionId = 0;
            IntPtr pServer = IntPtr.Zero;
            IntPtr pAddress = IntPtr.Zero;
            uint iReturned = 0;

            if (pid <= 0) {
                return username;
            }

            try { 
                bool retval = ProcessIdToSessionId(pid, out pSessionId);
                if (retval == true)
                {
                    if (WTSQuerySessionInformation(pServer,
                          (int)pSessionId, WTS_INFO_CLASS.WTSUserName,
                          out pAddress, out iReturned) == true)
                    {
                        username = Marshal.PtrToStringAnsi(pAddress);
                    }
                }

            }
            catch (Exception e)
            {
                username = String.Empty;
            }


            return username;
        }


        public bool GetSuspiciousLocations(out List<String> locations, uint pid)
        {
            bool retval = true;
            ///locations = null;
            locations = new List<String>();

            string username = GetUserForPid(pid);
            if (username != String.Empty)
            {
                string temp = String.Format(Forensics.ConstantVariables.APPDATA_DIR_FORMAT, username);
                locations.Add(temp);

                locations.Add(Forensics.ConstantVariables.SYSTEM32_DIR);
                locations.Add(Forensics.ConstantVariables.SYSWOW64_DIR);
            }
            else
            {
                locations.Add(Forensics.ConstantVariables.USER_DIR);
                locations.Add(Forensics.ConstantVariables.SYSTEM32_DIR);
                locations.Add(Forensics.ConstantVariables.SYSWOW64_DIR);
            }


            return retval;
        }




        public string GetUserName()
        {
            string strUserName = "";
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(ConstantVariables.SCOPE, ConstantVariables.QUERY_STRING))
                {
                    using (ManagementObjectCollection collection = searcher.Get())
                    {
                        strUserName = (string)collection.Cast<ManagementBaseObject>().First()[ConstantVariables.USERNAME];
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message + ConstantVariables.COLON + ex.StackTrace);
                strUserName = Environment.UserName;
            }

            return strUserName;
        }
    }
}
