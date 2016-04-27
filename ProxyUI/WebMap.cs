using System;
using System.Collections.Generic;

namespace ProxyUI
{
    public class WebMap
    {
        public static Dictionary<string, string> PathMap;
        public static Dictionary<string, string> MimeMap;

        public static readonly String WEB_MAP_DEF_PATH = "/";
        public static readonly String WEB_MAP_DEF_FILE = "proxyui.html";
        public static readonly String WEB_MAP_DEF_MIME = "text/html";


        public static readonly String WEB_MAP_CSS_MIME = "text/css";
        public static readonly String WEB_MAP_JS_MIME = "application/javascript";
        public static readonly String WEB_MAP_JPEG_MIME = "image/jpeg";
        public static readonly String WEB_MAP_PNG_MIME = "image/png";

        public static String wbroot;

        public WebMap()
        {
            PathMap = new Dictionary<string, string>();

            if (PathMap != null)
            {
                PathMap.Add(WEB_MAP_DEF_PATH, WEB_MAP_DEF_FILE);
            }

            MimeMap = new Dictionary<string, string>();
            if (MimeMap != null)
            {
                MimeMap.Add(WEB_MAP_DEF_PATH, WEB_MAP_DEF_MIME);
            }

            wbroot = System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                               ConstVars.APP_DATA_FOLDER), ConstVars.WEB_ROOT_FOLDER);
            //wbroot = @"C:\ProgramData\ProxyService\proxyui";

        }

        public string GetPath(String webpath)
        {
            string retpath = "";
            string fullpath = "";

            if (PathMap.TryGetValue(webpath, out retpath) != false)
            {
                if (retpath[0] != '\\')
                {
                    fullpath = wbroot + "\\" + retpath;
                }
                else
                {
                    fullpath = wbroot + retpath;
                }

                return fullpath;
            }
            string webpathnew = webpath.Replace('/', '\\');

            if (webpathnew == null)
                return "";

            if (fullpath == "")
            {
                fullpath = wbroot + webpathnew;
            }

            return fullpath;
        }

        public bool GetMimeByExtension(String webpath, out String mimetype)
        {
            String[] splitBuffer = webpath.Split(ConstVars.DOT_Split);

            int count = splitBuffer.Length;

            if (count > 1)
            {

                if (splitBuffer[count - 1] != "")
                {
                    string tempmime = splitBuffer[count - 1].ToLower();
                    if (tempmime.Equals("css") == true)
                    {
                        mimetype = WEB_MAP_CSS_MIME;
                        return true;
                    }
                    if (tempmime.Equals("js") == true)
                    {
                        mimetype = WEB_MAP_JS_MIME;
                        return true;
                    }
                    if (tempmime.Equals("jpg") == true)
                    {
                        mimetype = WEB_MAP_JPEG_MIME;
                        return true;
                    }
                    if (tempmime.Equals("png") == true)
                    {
                        mimetype = WEB_MAP_PNG_MIME;
                        return true;
                    }
                }
            }
            mimetype = WEB_MAP_DEF_MIME;

            return true;
        }

        public string GetMime(String webpath)
        {
            string mimetype = "";

            if (MimeMap.TryGetValue(webpath, out mimetype) != false)
            {
                return mimetype;
            }
            else
                if (GetMimeByExtension(webpath, out mimetype) != false)
                {
                    return mimetype;
                }

            return WEB_MAP_DEF_MIME;
        }
    }
}
