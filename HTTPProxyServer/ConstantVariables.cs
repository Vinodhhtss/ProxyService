using System;
using System.Text.RegularExpressions;

namespace HTTPProxyServer
{
    public class ConstantVariables
    {
        public static readonly String[] COLON_SPACE_SPLIT = new string[] { ": " };
        public static readonly String[] SEMI_COLON_SPACE_SPLIT = new string[] { "; " };
        public static readonly char[] QUESTION_SPLIT = new char[] { '?' };
        public static readonly char[] SEMI_SPLIT = new char[] { ';' };
        public static readonly char[] EQUAL_SPLIT = new char[] { '=' };
        public static readonly char[] AMP_SPLIT = new char[] { '&' };
        public static readonly char[] SPACE_Split = new char[] { ' ' };
        public static readonly char[] COMMA_SPLIT = new char[] { ',' };
        public static readonly Regex COOKIE_SPLIT_REGEX = new Regex(@",(?! )");
        public static readonly char[] XML_NAME_INVALID_CHAR = new char[] { ':', '.' };
        public const char COLON = ':';

        public static readonly int BUFFER_SIZE = 8192;

        public const string DATE_FORMAT = "yyyyMMddHHmmssfff";

        public const string LUA_SCRIPTS_LOG_FOLDER = "LuaScriptsLog";
        public const string LUA_SCRIPTS_SEARCH_PATTERN = "*.lua";

        //Comment strings 

        public const string XML_EXTENSION = ".xml";
        public const string TEXT_EXTENSION = ".txt";

        //Rajesh chages 
        public const string REQ = " - REQ - ";

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

        //Rajesh
        public const string PROXY_UI_URL = "http://www.spellsecurity.com/";


        //Pipe Communication  
        public const string PIPE_SERVER_NAME = "ProxyServicePipe";

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
