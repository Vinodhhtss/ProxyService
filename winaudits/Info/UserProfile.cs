using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;

namespace winaudits
{

    public class User
    {
        [JsonProperty("username")]
        public string UserName { get; set; }
        [JsonProperty("sid")]
        public string SID { get; set; }
        [JsonProperty("fullname")]
        public string FullName { get; set; }
        [JsonProperty("domain")]
        public string Domain { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("sidtype")]
        public string SIDType { get; set; }
        [JsonProperty("lastlogin")]
        public DateTime LastLogin { get; set; }
        [JsonProperty("isdisabled")]
        public bool IsDisabled { get; set; }
        [JsonProperty("islocked")]
        public bool IsLocked { get; set; }
        [JsonProperty("passwordrequired")]
        public bool PasswordRequired { get; set; }
        [JsonProperty("passwordage")]
        public string PasswordAge { get; set; }
        [JsonProperty("groups")]
        public string Groups { get; set; }

    }

    public class UserProfileAuditor
    {
        const int FILTER_TEMP_DUPLICATE_ACCOUNT = 0x0001;
        const int FILTER_NORMAL_ACCOUNT = 0x0002;
        const int FILTER_INTERDOMAIN_TRUST_ACCOUNT = 0x0008;
        const int FILTER_WORKSTATION_TRUST_ACCOUNT = 0x0010;
        const int FILTER_SERVER_TRUST_ACCOUNT = 0x0020;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct USER_INFO_2
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string usri2_name;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string usri2_password;
            public int usri2_password_age;
            public int usri2_priv;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string usri2_home_dir;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string usri2_comment;
            public uint usri2_flags;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string usri2_script_path;
            public uint usri2_auth_flags;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string usri2_full_name;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string usri2_usr_comment;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string usri2_parms;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string usri2_workstations;
            public uint usri2_last_logon;
            public uint usri2_last_logoff;
            public uint usri2_acct_expires;
            public int usri2_max_storage;
            public int usri2_units_per_week;
            IntPtr usri2_logon_hours;
            public uint usri2_bad_pw_count;
            public uint usri2_num_logons;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string usri2_logon_server;
            public int usri2_country_code;
            public int usri2_code_page;
        }

        [DllImport("Netapi32.dll")]
        private extern static int NetUserEnum(string servername, int level, int filter, out IntPtr bufptr, int prefmaxlen,
                                     out int entriedread, out int totalentries, out int resume_handle);


        // NetAPIBufferFree - Used to clear the Network buffer after NetUserEnum
        [DllImport("Netapi32.dll")]
        private extern static int NetApiBufferFree(IntPtr Buffer);

        [DllImport("Netapi32.dll", SetLastError = true)]
        private extern static int NetUserGetGroups
            ([MarshalAs(UnmanagedType.LPWStr)] string servername,
             [MarshalAs(UnmanagedType.LPWStr)] string username,
             int level,
             out IntPtr bufptr,
             UInt32 prefmaxlen,
             out int entriesread,
             out int totalentries);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct LOCALGROUP_USERS_INFO_0
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string name;
        }


        [DllImport("Netapi32.dll", SetLastError = true)]
        private extern static int NetUserGetLocalGroups([MarshalAs(UnmanagedType.LPWStr)] string servername,
            [MarshalAs(UnmanagedType.LPWStr)] string username,
            int level,
            int flags,
            out IntPtr bufptr,
            int prefmaxlen,
            out int entriesread,
            out int totalentries);


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
        static extern bool LookupAccountName(
            string lpSystemName,
            string lpAccountName,
            [MarshalAs(UnmanagedType.LPArray)] byte[] Sid,
            ref uint cbSid,
            StringBuilder ReferencedDomainName,
            ref uint cchReferencedDomainName,
            out SID_NAME_USE peUse);

        [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool ConvertSidToStringSid(
            [MarshalAs(UnmanagedType.LPArray)] byte[] pSID,
            out IntPtr ptrSid);

        [DllImport("kernel32.dll")]
        static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private extern static int NetUserGetInfo(
            [MarshalAs(UnmanagedType.LPWStr)] string ServerName,
            [MarshalAs(UnmanagedType.LPWStr)] string UserName,
            int level,
            out IntPtr BufPtr);

        static long UF_ACCOUNTDISABLE = 0x000002;
        static long UF_LOCKOUT = 0x10;
        static long UF_PASSWD_NOTREQD = 0x20;

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode)]
        static extern NetApiStatus NetGetDCName(string serverName, string domainName, out IntPtr buffer);

        public enum NetApiStatus
        {
            Success = 0,
            DCNotFound = 2453
        }


        public static string GetDCName()
        {
            IntPtr domainInfo = IntPtr.Zero;
            string dcName = String.Empty;

            try
            {
                NetApiStatus result = NetGetDCName(null, null, out domainInfo);
                if (result == NetApiStatus.Success)
                {
                    dcName = Marshal.PtrToStringAuto(domainInfo);
                }
            }
            finally
            {
                NetApiBufferFree(domainInfo);
            }

            return dcName;

        }

        private static UserProfileAuditor.USER_INFO_2 GetDomainUserInfo(string serverName, string userName)
        {
            USER_INFO_2 objUserInfo2 = new USER_INFO_2();

            try
            {
                IntPtr bufPtr; // because it's an OUT, we don't need to Alloc
                int lngReturn = NetUserGetInfo(serverName, userName, 10, out bufPtr);
                if (lngReturn == 0)
                {
                    objUserInfo2 = (USER_INFO_2)Marshal.PtrToStructure(bufPtr, typeof(USER_INFO_2));
                }
                NetApiBufferFree(bufPtr);
                bufPtr = IntPtr.Zero;
            }
            catch (Exception)
            {
            }

            return objUserInfo2;
        }



        public static List<User> StartAudit()
        {
            int EntriesRead;
            int TotalEntries;
            int Resume;

            IntPtr bufPtr;
            List<User> lstUser = new List<User>();

            UserProfileAuditor.NetUserEnum(null, 2, 0,
                       out bufPtr, -1, out EntriesRead, out TotalEntries, out Resume);
            int err = Marshal.GetLastWin32Error();
            List<string> lstProfiles = RegistryUtil.GetUserProfiles();

            if (EntriesRead > 0)
            {
                UserProfileAuditor.USER_INFO_2[] Users = new UserProfileAuditor.USER_INFO_2[EntriesRead];
                IntPtr iter = bufPtr;
                for (int i = 0; i < EntriesRead; i++)
                {
                    Users[i] = (UserProfileAuditor.USER_INFO_2)Marshal.PtrToStructure(iter, typeof(UserProfileAuditor.USER_INFO_2));
                    iter = (IntPtr)((int)iter + Marshal.SizeOf(typeof(UserProfileAuditor.USER_INFO_2)));

                    User user = new User();
                    user.UserName = Users[i].usri2_name;
                    string localGroup = string.Empty;
                    foreach (var item in GetLocalGroups(user.UserName))
                    {
                        localGroup += item + ";";
                    }
                    user.Groups = localGroup.TrimEnd(new char[] { ';' });
                    user.FullName = Users[i].usri2_full_name;
                    user.PasswordAge = Users[i].usri2_password_age.ToString();
                    user.Description = Users[i].usri2_comment;
                    user.LastLogin = GetTimeFormElaspedSeconds((uint)Users[i].usri2_last_logon);
                    user.IsDisabled = CheckFlagIsEnabled(Users[i].usri2_flags, UF_ACCOUNTDISABLE);
                    user.IsLocked = CheckFlagIsEnabled(Users[i].usri2_flags, UF_LOCKOUT);
                    user.PasswordRequired = !CheckFlagIsEnabled(Users[i].usri2_flags, UF_PASSWD_NOTREQD);

                    GetSidDetails(user);
                    if (lstProfiles.Contains(user.SID))
                    {
                        lstProfiles.Remove(user.SID);
                    }
                    lstUser.Add(user);
                }

            }


            string serverName = GetDCName();
            foreach (string item in lstProfiles)
            {
                string userName = string.Empty;
                try
                {
                    userName = new SecurityIdentifier(item).Translate(typeof(NTAccount)).ToString();

                    User user = new User();

                    user.UserName = userName;
                    string localGroup = string.Empty;
                    foreach (var group in GetLocalGroups(user.UserName))
                    {
                        localGroup += group + ";";
                    }
                    user.Groups = localGroup.TrimEnd(new char[] { ';' });
                    string[] usersNme = user.UserName.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    GetSidDetails(user);
                    try
                    {
                        if (!string.IsNullOrEmpty(serverName))
                        {
                            USER_INFO_2 userInfo2 = GetDomainUserInfo(serverName, user.UserName);
                            user.FullName = userInfo2.usri2_full_name;
                            user.FullName = userInfo2.usri2_full_name;
                            user.PasswordAge = userInfo2.usri2_password_age.ToString();
                            user.Description = userInfo2.usri2_comment;
                            user.LastLogin = GetTimeFormElaspedSeconds((uint)userInfo2.usri2_last_logon);
                            user.IsDisabled = CheckFlagIsEnabled(userInfo2.usri2_flags, UF_ACCOUNTDISABLE);
                            user.IsLocked = CheckFlagIsEnabled(userInfo2.usri2_flags, UF_LOCKOUT);
                            user.PasswordRequired = !CheckFlagIsEnabled(userInfo2.usri2_flags, UF_PASSWD_NOTREQD);
                        }

                    }
                    catch (Exception)
                    {
                    }

                    lstUser.Add(user);

                }
                catch (Exception)
                {
                }
            }

            UserProfileAuditor.NetApiBufferFree(bufPtr);
            return lstUser;
        }

        public static bool CheckFlagIsEnabled(uint flag, long flagToCheck)
        {
            try
            {
                Int32 intFlagExists = 0;
                intFlagExists = Convert.ToInt32(flag & flagToCheck);
                if (intFlagExists > 0)
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        private static List<string> GetLocalGroups(string Username)
        {
            List<string> myList = new List<string>();
            int EntriesRead;
            int TotalEntries;
            IntPtr bufPtr;
            string ServerName = null;
            int Flags = 0;

            NetUserGetLocalGroups(ServerName, Username, 0, Flags, out bufPtr, 1024, out EntriesRead, out TotalEntries);

            if (EntriesRead > 0)
            {
                LOCALGROUP_USERS_INFO_0[] RetGroups = new LOCALGROUP_USERS_INFO_0[EntriesRead];
                IntPtr iter = bufPtr;
                for (int i = 0; i < EntriesRead; i++)
                {
                    RetGroups[i] = (LOCALGROUP_USERS_INFO_0)Marshal.PtrToStructure(iter, typeof(LOCALGROUP_USERS_INFO_0));
                    iter = (IntPtr)((int)iter + Marshal.SizeOf(typeof(LOCALGROUP_USERS_INFO_0)));
                    myList.Add(RetGroups[i].name);
                }
                NetApiBufferFree(bufPtr);
            }
            return myList;
        }

        private static DateTime GetTimeFormElaspedSeconds(uint seconds)
        {
            //    00:00:00, January 1, 1970
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            try
            {
                dt = dt.AddSeconds(seconds);
            }
            catch (Exception)
            {
            }
            return dt.ToUniversalTime();
        }

        private static void GetSidDetails(User user)
        {
            byte[] Sid = null;
            uint cbSid = 0;
            StringBuilder referencedDomainName = new StringBuilder();
            uint cchReferencedDomainName = (uint)referencedDomainName.Capacity;
            SID_NAME_USE sidUse;

            int err = 0;
            if (!LookupAccountName(null, user.UserName, Sid, ref cbSid, referencedDomainName, ref cchReferencedDomainName, out sidUse))
            {
                err = Marshal.GetLastWin32Error();
                if (err == 122 || err == 1004)
                {
                    Sid = new byte[cbSid];
                    referencedDomainName.EnsureCapacity((int)cchReferencedDomainName);
                    err = 0;
                    if (!LookupAccountName(null, user.UserName, Sid, ref cbSid, referencedDomainName, ref cchReferencedDomainName, out sidUse))
                        err = Marshal.GetLastWin32Error();

                    if (err == 0)
                    {
                        IntPtr ptrSid;
                        if (ConvertSidToStringSid(Sid, out ptrSid))
                        {
                            user.SID = Marshal.PtrToStringAuto(ptrSid);
                            user.SIDType = sidUse.ToString();
                        }
                    }
                }
            }
            else
            {
                // Consider throwing an exception since no result was found
            }
        }
    }
}
