using System.Collections.Generic;

namespace HTTPProxyServer
{
    public class ClientNameAlert : IAlert
    {
        public static List<string> ClientNameEntries = new List<string>();

        static ClientNameAlert()
        {
            ClientNameEntries.Add("acrord32.exe");
            ClientNameEntries.Add("winword.exe");
            ClientNameEntries.Add("excel.exe");
            ClientNameEntries.Add("winword");
            ClientNameEntries.Add("powerpoint");
            ClientNameEntries.Add("excel");
        }

        public bool IsAlert(SessionHandler oSessionHndlr)
        {

            string clname = oSessionHndlr.ClientName.ToLower();

            for (int i = 0; i < ClientNameEntries.Count; i++)
            {
                if (clname.Contains(ClientNameEntries[i]) == true)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
