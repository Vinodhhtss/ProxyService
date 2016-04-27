using System.Collections.Generic;
using System.Linq;

namespace HTTPProxyServer
{
    class ContentTypes : IAlert
    {
        public static Dictionary<string, string> Content_Type = new Dictionary<string, string>();

        static ContentTypes()
        {
            Content_Type.Add("doc", "application/msword");
            Content_Type.Add("docx", "application/msword");
            Content_Type.Add("xls", "application/vnd.ms-excel");
            Content_Type.Add("xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            Content_Type.Add("ppt", "application/vnd.ms-powerpoint");
            Content_Type.Add("pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation");
        }

        public bool IsAlert(SessionHandler oSessionHndlr)
        {
            if (Content_Type.Values.Contains(oSessionHndlr.ContentMimeType))
            {
                return true;
            }
            return false;
        }
    }
}
