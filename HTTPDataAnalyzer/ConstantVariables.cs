using System;

namespace HTTPDataAnalyzer
{
    public class ConstantVariables
    {
        //temp 
        public static bool IsInDebug = System.Diagnostics.Debugger.IsAttached;

        public static readonly String[] COLON_SPACE_SPLIT = new string[] { ": " };
        public static readonly String[] SEMI_COLON_SPACE_SPLIT = new string[] { "; " };
        public static readonly char[] QUESTION_SPLIT = new char[] { '?' };
        public static readonly char[] EQUAL_SPLIT = new char[] { '=' };
        public static readonly char[] AMP_SPLIT = new char[] { '&' };

        public const string CIRCULAR_BUFFER_HAR_FILE_NAME = "CircularBuffer.har";
        public const string HTTP_VERSION_10 = "HTTP/1.0";

        public const string HAR_CREATOR = "SpellSecurity Inc";
        public const string HAR_VERSION = "1.1";
        public const string COOKIE = "Cookie";

        public static string DOWLOAD_FOLDER_PATH;
        public static string UPLOAD_FOLDER_PATH;

        // public const string LUA_SCRIPTS_FOLDER = "LuaScripts";
        public const string LUA_SCRIPTS_LOG_FOLDER = "LuaScriptsLog";
        public const string LUA_SCRIPTS_SEARCH_PATTERN = "*.lua";     
        public const string TEXT_EXTENSION = ".txt";

        public const string DATE_FORMAT = "yyyyMMddHHmmssfff";

        // XML CONSTANT
        public const string PACKETDETAILS = "PacketDetails";
        public const string CLIENT = "Client";
        public const string PACKETDETAIL = "PacketDetail";
        public const string CLIENTID = "ClientID";
        public const string THREADINDEX = "ThreadIndex";
        public const string FIRSTHEADERLINE = "FirstHeaderLine";
        public const string REQUEST = "Request";
        public const string HEADERS = "Headers";
        public const string REQUESTBODY = "RequestBody";
        public const string RESPONSEBODY = "ResponseBody";
        public const string RESPONSE = "Response";


        public const string XML_EXTENSION = ".xml";
        public static readonly char[] XML_NAME_INVALID_CHAR = new char[] { ':', '.' };

        public const string DATETIMEFORMAT = "yyyy-MM-ddThh:mm:sszzz";

        //Pipe Communication  

        public static string PIPE_SERVER_NAME
        {
            get { return "ProxyServicePipe"; }
        } 

        public static string PIPE_DNS_SERVER_NAME
        {
            get { return "ProxyServiceDNSPipe"; }
        } 

        #region Appsetting
        public static string GetListeningIPInterface()
        {
            return "ListeningIPInterface";
        }

        public static string GetListeningPort()
        {
            return "ListeningPort";
        }
        #endregion

        public static string GetAppDataFolder()
        {
            return "ProxyService";
        }

        public static string GetLuaScriptsFolder()
        {
            return "LuaScripts";
        }
    }
}
