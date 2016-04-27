using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;

namespace Forensics
{
    public class ConstantVariables
    {
        public const string USER_DIR = "C:\\Users\\";
        public const string APPDATA_DIR_FORMAT = "C:\\Users\\{0}\\AppData\\";
        public const string SYSTEM32_DIR = "C:\\Windows\\System32\\";
        public const string SYSWOW64_DIR = "C:\\Windows\\SySWOW64\\";



        public const string SCOPE = @"\root\CIMV2";
        public const string QUERY_STRING = "SELECT UserName FROM Win32_ComputerSystem";
        public const string USERNAME = "UserName";
        public const char COLON = ':';


        /////////USN queue count
        public const uint MAX_USN_QUEUE = 1000;

       

        
    }
}
