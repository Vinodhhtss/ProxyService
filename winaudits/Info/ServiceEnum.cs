using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace winaudits
{

    public class Services
    {
        [JsonProperty("servicename")]
        public string ServiceName { get; set; }
        [JsonProperty("fullname")]
        public string FullName { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("arguments")]
        public string Arguments { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("startmode")]
        public string StartMode { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("servicedll")]
        public string ServiceDLL { get; set; }
        [JsonProperty("startedas")]
        public string StartedAs { get; set; }
        [JsonProperty("pid")]
        public int PID { get; set; }
        [JsonProperty("md5ofexecutable")]
        public string MD5OfExecutable { get; set; }
        [JsonProperty("md5servicedll")]
        public string MD5ServiceDLL { get; set; }
        [JsonProperty("executablesigned")]
        public bool ExecutableSigned { get; set; }
        [JsonProperty("executablesignverified")]
        public bool ExecutableSignVerified { get; set; }
        [JsonProperty("executablecertissuer")]
        public string ExecutableCertIssuer { get; set; }
        [JsonProperty("executablecertsubject")]
        public string ExecutableCertSubject { get; set; }
        [JsonProperty("dllsigned")]
        public bool DLLSigned { get; set; }
        [JsonProperty("dllsignedverified")]
        public bool DLLSignedVerified { get; set; }
        [JsonProperty("dllcertissuer")]
        public string DLLCertIssuer { get; set; }
        [JsonProperty("dllcertsubject")]
        public string DLLCertSubject { get; set; }
    }

    public class ServiceEnum
    {
        enum ServiceStartMode
        {
            Undefinied = 0,
            AutomaticDelayed,
            Automatic,
            Manual,
            Disabled
        }


        [StructLayout(LayoutKind.Sequential)]
        struct SERVICE_STATUS_PROCESS
        {
            public int serviceType;
            public int currentState;
            public int controlsAccepted;
            public int win32ExitCode;
            public int serviceSpecificExitCode;
            public int checkPoint;
            public int waitHint;
            public int processId;
            public int serviceFlags;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct ENUM_SERVICE_STATUS_PROCESS
        {
            internal static readonly int SizePack4 = Marshal.SizeOf(typeof(ENUM_SERVICE_STATUS_PROCESS));

            /// <summary>
            /// sizeof(ENUM_SERVICE_STATUS_PROCESS) allow Packing of 8 on 64 bit machines
            /// </summary>
            internal static readonly int SizePack8 = Marshal.SizeOf(typeof(ENUM_SERVICE_STATUS_PROCESS)) + 4;

            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            internal string pServiceName;

            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            internal string pDisplayName;

            internal SERVICE_STATUS_PROCESS ServiceStatus;
        }

        [StructLayout(LayoutKind.Sequential)]
        class QUERY_SERVICE_CONFIG
        {
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
            public UInt32 dwServiceType;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
            public UInt32 dwStartType;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
            public UInt32 dwErrorControl;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public String lpBinaryPathName;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public String lpLoadOrderGroup;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)]
            public UInt32 dwTagID;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public String lpDependencies;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public String lpServiceStartName;
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public String lpDisplayName;
        }

        [StructLayout(LayoutKind.Sequential)]
        class SERVICE_DESCRIPTION
        {
            [MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
            public String lpDescription;
        }

        private const uint SERVICE_NO_CHANGE = unchecked(0xffffffff); //this value is found in winsvc.h
        private const uint STANDARD_RIGHTS_REQUIRED = 0x000F0000;
        private const int SC_ENUM_PROCESS_INFO = 0;

        private enum ServiceType
        {
            SERVICE_KERNEL_DRIVER = 0x1, SERVICE_FILE_SYSTEM_DRIVER = 0x2,
            SERVICE_WIN32_OWN_PROCESS = 0x10, SERVICE_WIN32_SHARE_PROCESS = 0x20,
            SERVICE_INTERACTIVE_PROCESS = 0x100, SERVICETYPE_NO_CHANGE = unchecked((int)SERVICE_NO_CHANGE),
            SERVICE_WIN32 = (SERVICE_WIN32_OWN_PROCESS | SERVICE_WIN32_SHARE_PROCESS)
        }

        private enum ServiceStateRequest { SERVICE_ACTIVE = 0x1, SERVICE_INACTIVE = 0x2, SERVICE_STATE_ALL = (SERVICE_ACTIVE | SERVICE_INACTIVE) }

        private enum ServiceControlManagerType { SC_MANAGER_CONNECT = 0x1, SC_MANAGER_CREATE_SERVICE = 0x2, SC_MANAGER_ENUMERATE_SERVICE = 0x4, SC_MANAGER_LOCK = 0x8, SC_MANAGER_QUERY_LOCK_STATUS = 0x10, SC_MANAGER_MODIFY_BOOT_CONFIG = 0x20, SC_MANAGER_ALL_ACCESS = unchecked((int)(STANDARD_RIGHTS_REQUIRED | SC_MANAGER_CONNECT | SC_MANAGER_CREATE_SERVICE | SC_MANAGER_ENUMERATE_SERVICE | SC_MANAGER_LOCK | SC_MANAGER_QUERY_LOCK_STATUS | SC_MANAGER_MODIFY_BOOT_CONFIG)) }


        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr OpenSCManager(string machineName, string databaseName, uint dwAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern Boolean QueryServiceConfig(IntPtr hService, IntPtr intPtrQueryConfig, int cbBufSize, out int pcbBytesNeeded);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "QueryServiceConfig2W")]
        static extern Boolean QueryServiceConfig2(IntPtr hService, UInt32 dwInfoLevel, IntPtr buffer, UInt32 cbBufSize, out UInt32 pcbBytesNeeded);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr OpenService(IntPtr hSCManager, String lpServiceName, UInt32 dwDesiredAccess);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool EnumServicesStatusEx(IntPtr hSCManager,
        int infoLevel, int dwServiceType,
        int dwServiceState, IntPtr lpServices, UInt32 cbBufSize,
        out uint pcbBytesNeeded, out uint lpServicesReturned,
        ref uint lpResumeHandle, string pszGroupName);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseServiceHandle(IntPtr hSCObject);


        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        internal static List<Services> GetServices()
        {
            var serviceList = new List<Services>();

            List<ENUM_SERVICE_STATUS_PROCESS> result = new List<ENUM_SERVICE_STATUS_PROCESS>();

            IntPtr handle = IntPtr.Zero;
            IntPtr buf = IntPtr.Zero;
            try
            {
                handle = OpenSCManager(null, null, (int)ServiceControlManagerType.SC_MANAGER_ALL_ACCESS);
                if (handle != IntPtr.Zero)
                {
                    uint iBytesNeeded = 0;
                    uint iServicesReturned = 0;
                    uint iResumeHandle = 0;

                    //  ENUM_SERVICE_STATUS_PROCESS infoLevel = new ENUM_SERVICE_STATUS_PROCESS();
                    if (!EnumServicesStatusEx(handle, SC_ENUM_PROCESS_INFO, (int)ServiceType.SERVICE_WIN32, (int)ServiceStateRequest.SERVICE_STATE_ALL, IntPtr.Zero, 0, out iBytesNeeded, out iServicesReturned, ref iResumeHandle, null))
                    {
                        // allocate our memory to receive the data for all the services (including the names)
                        buf = Marshal.AllocHGlobal((int)iBytesNeeded);

                        if (!EnumServicesStatusEx(handle, SC_ENUM_PROCESS_INFO, (int)ServiceType.SERVICE_WIN32, (int)ServiceStateRequest.SERVICE_STATE_ALL, buf, iBytesNeeded, out iBytesNeeded, out iServicesReturned, ref iResumeHandle, null))
                            return null;

                        ENUM_SERVICE_STATUS_PROCESS serviceStatus;

                        // check if 64 bit system which has different pack sizes
                        if (IntPtr.Size == 8)
                        {
                            long pointer = buf.ToInt64();
                            for (int i = 0; i < (int)iServicesReturned; i++)
                            {
                                serviceStatus = (ENUM_SERVICE_STATUS_PROCESS)Marshal.PtrToStructure(new IntPtr(pointer),
                                   typeof(ENUM_SERVICE_STATUS_PROCESS));
                                result.Add(serviceStatus);

                                // incremement by sizeof(ENUM_SERVICE_STATUS_PROCESS) allow Packing of 8
                                pointer += ENUM_SERVICE_STATUS_PROCESS.SizePack8;
                            }
                        }
                        else
                        {
                            int pointer = buf.ToInt32();
                            for (int i = 0; i < (int)iServicesReturned; i++)
                            {
                                serviceStatus = (ENUM_SERVICE_STATUS_PROCESS)Marshal.PtrToStructure(new IntPtr(pointer),
                                   typeof(ENUM_SERVICE_STATUS_PROCESS));
                                result.Add(serviceStatus);

                                // incremement by sizeof(ENUM_SERVICE_STATUS_PROCESS) allow Packing of 4
                                pointer += ENUM_SERVICE_STATUS_PROCESS.SizePack4;
                            }
                        }
                    }

                    for (int i = 0; i < result.Count; i++)
                    {
                        ENUM_SERVICE_STATUS_PROCESS service = result[i];
                        Services srv = new Services();
                        if (srv != null)
                        {
                            srv.ServiceName = service.pServiceName;
                            srv.FullName = service.pDisplayName;
                            int sstatus = service.ServiceStatus.currentState;
                            int stype = service.ServiceStatus.serviceType;
                            srv.PID = service.ServiceStatus.processId;


                            switch (sstatus)
                            {
                                case 0x2:
                                case 0x4:
                                case 0x5:
                                case 0x6:
                                case 0x7:
                                    srv.Status = "SERVICE_ACTIVE";
                                    break;
                                case 0x01:
                                    srv.Status = "SERVICE_INACTIVE";
                                    break;
                                default:
                                    //case 0x3:
                                    srv.Status = "UNKNOWN";
                                    break;
                            }

                            switch (stype)
                            {
                                case 0x01:
                                    srv.Type = "SERVICE_KERNEL_DRIVER";
                                    break;

                                case 0x02:
                                    srv.Type = "SERVICE_FILE_SYSTEM_DRIVER";
                                    break;

                                case 0x10:
                                    srv.Type = "SERVICE_WIN32_OWN_PROCESS";
                                    break;

                                case 0x20:
                                    srv.Type = "SERVICE_WIN32_SHARED_PROCESS";
                                    break;

                                case 0x100:
                                    srv.Type = "SERVICE_INTERACTIVE_PROCESS";
                                    break;

                                default:
                                    srv.Type = "UNKNOWN";
                                    break;
                            }
                            try
                            {
                                IntPtr serviceHandle = OpenService(handle, service.pServiceName, 0x1);

                                QUERY_SERVICE_CONFIG qUERY_SERVICE_CONFIG = new QUERY_SERVICE_CONFIG();
                                IntPtr ptr = Marshal.AllocHGlobal(0);
                                int bytesNeeded = 0;
                                bool success = QueryServiceConfig(serviceHandle, ptr, 0, out bytesNeeded);
                                if (bytesNeeded != 0)
                                {
                                    ptr = Marshal.AllocHGlobal(bytesNeeded);
                                    success = QueryServiceConfig(serviceHandle, ptr,
                                    bytesNeeded, out bytesNeeded);

                                    Marshal.PtrToStructure(ptr, qUERY_SERVICE_CONFIG);

                                    Marshal.FreeHGlobal(ptr);

                                }
                                srv.Path = qUERY_SERVICE_CONFIG.lpBinaryPathName;
                                srv.StartMode = ((ServiceStartMode)qUERY_SERVICE_CONFIG.dwStartType).ToString();
                                srv.Description = GetServiceDescription(serviceHandle);
                                srv.StartedAs = qUERY_SERVICE_CONFIG.lpServiceStartName;
                            }
                            catch (Exception)
                            {

                            }
                            serviceList.Add(srv);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (handle != IntPtr.Zero)
                    CloseServiceHandle(handle);

                if (buf != IntPtr.Zero)
                    Marshal.FreeHGlobal(buf);
            }

            return serviceList;
        }

        static private string GetServiceDescription(IntPtr serviceHandle)
        {
            SERVICE_DESCRIPTION descriptionStruct = new SERVICE_DESCRIPTION();
            try
            {
                UInt32 dwBytesNeeded;

                // Determine the buffer size needed
                bool sucess = QueryServiceConfig2(serviceHandle, 0x01, IntPtr.Zero, 0, out dwBytesNeeded);

                IntPtr ptr = Marshal.AllocHGlobal((int)dwBytesNeeded);
                sucess = QueryServiceConfig2(serviceHandle, 0x01, ptr, dwBytesNeeded, out dwBytesNeeded);

                Marshal.PtrToStructure(ptr, descriptionStruct);
                Marshal.FreeHGlobal(ptr);

            }
            catch (Exception)
            {
            }

            return descriptionStruct.lpDescription;
            // Report it.
        }

        public static List<Services> StartAudit()
        {
            var serviceList = ServiceEnum.GetServices();
            for (int i = 0; i < serviceList.Count; i++)
            {
                Services srv = serviceList[i];

                string[] pathAndArgument = srv.Path.Split(new string[] { " -", " /", " \"" }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (pathAndArgument.Length == 1)
                {

                    srv.Path = pathAndArgument[0].Replace("\"", string.Empty);
                }
                else if (pathAndArgument.Length == 2)
                {
                    srv.Path = pathAndArgument[0].Replace("\"", string.Empty);
                    srv.Arguments = pathAndArgument[1].Replace("\"", string.Empty);
                }

                Forensics.CryptInfo ci;

                ci = Forensics.SigVerify.CheckSignatureForFile(srv.Path);
                ci.MD5 = Forensics.ProxyMD5.ComputeFileMD5(srv.Path);

                srv.MD5OfExecutable = ci.MD5;
                srv.ExecutableSigned = ci.IsSigned;
                srv.ExecutableSignVerified = ci.IsVerified;
                srv.ExecutableCertSubject = ci.Subject;
                srv.ExecutableCertIssuer = ci.CA;

                srv.ServiceDLL = GetServiceDLL(srv.ServiceName);
                if (!string.IsNullOrEmpty(srv.ServiceDLL))
                {
                    Forensics.CryptInfo ciServiceDLL;

                    ciServiceDLL = Forensics.SigVerify.CheckSignatureForFile(srv.ServiceDLL);
                    ciServiceDLL.MD5 = Forensics.ProxyMD5.ComputeFileMD5(srv.ServiceDLL);

                    srv.MD5ServiceDLL = ciServiceDLL.MD5;
                    srv.DLLSigned = ciServiceDLL.IsSigned;
                    srv.DLLSignedVerified = ciServiceDLL.IsVerified;
                    srv.DLLCertSubject = ciServiceDLL.Subject;
                    srv.DLLCertIssuer = ciServiceDLL.CA;
                }
            }
            return serviceList;
        }

        private static string GetServiceDLL(string serviceName)
        {
            string serviceDll = string.Empty;
            try
            {
                string serviceKey = "SYSTEM\\CurrentControlSet\\services\\" + Path.GetFileNameWithoutExtension(serviceName) + "\\Parameters";
                using (RegistryKey serviceParams = Registry.LocalMachine.OpenSubKey(serviceKey))
                {
                    if (serviceParams != null)
                    {
                        serviceDll = serviceParams.GetValue("ServiceDLL", string.Empty).ToString();
                    }
                }
            }
            catch (Exception)
            {
            }

            return serviceDll;
        }
    }
}
