using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace HTTPDataAnalyzer
{ 
    public class MIMEIdentifiers : IConditionChecker
    {
        public string ExploitExeMIME = "application/x-msdownload";
        public SessionHandler DBSessionHandler;      

        public bool CheckCondition(SessionHandler oSessionHandler)
        {
            DateTime dateForFilter = DateTime.Now;
            dateForFilter = dateForFilter.AddDays(-3);
            DataTable dt = AnalyzerManager.ProxydbObj.GetTableFromDB("select * from PacketDetails where starttime >" + "'" + dateForFilter.Date.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'", "proxyloghar");

            if (dt.Rows.Count <= 0)
            {
                return false;
            }
            Queue<SessionHandler> tempBuffer = PacketCreator.CreatePackets(dt);

            foreach (SessionHandler innnerSessionHandler in tempBuffer)
            {
                if (File.Exists(oSessionHandler.ClientName))
                {
                    if (ComputeFileMD5(oSessionHandler.ClientName) == GetMd5Hash(innnerSessionHandler.ResponseRawData))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        public bool IsMalicious(SessionHandler currentSessionHandler, SessionHandler inQueueSessionHandler)
        {
            if (ComputeFileMD5(currentSessionHandler.ClientName) == GetMd5Hash(inQueueSessionHandler.ResponseRawData))
            {
                DBSessionHandler = inQueueSessionHandler;
                return true;
            }
            return false;
        }

        //public void SendMessage(SessionHandler oSessionHandler)
        //{
        //    TCPClients.SendAlertMessageToServer("1", oSessionHandler, null);
        //}

       

        public string ComputeFileMD5(String filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    byte[] data = md5.ComputeHash(stream);
                    StringBuilder sBuilder = new StringBuilder();

                    // Loop through each byte of the hashed data  
                    // and format each one as a hexadecimal string. 
                    for (int i = 0; i < data.Length; i++)
                    {
                        sBuilder.Append(data[i].ToString("x2"));
                    }

                    // Return the hexadecimal string. 
                    return sBuilder.ToString();
                }
            }
        }

        static string GetMd5Hash(byte[] dataInput)
        {
            using (var md5 = MD5.Create())
            {

                byte[] data = md5.ComputeHash(dataInput);
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data  
                // and format each one as a hexadecimal string. 
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                // Return the hexadecimal string. 
                return sBuilder.ToString();
            }
        }
    }
}
