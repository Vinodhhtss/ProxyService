using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace HTTPDataAnalyzer
{
    class MessageProcessor
    {
        public static void ProcessMessage(string message)
        {
            int primaryKey = int.Parse(message);
            bool isRowExist = true;
            string rowCheck = "select * from Request INNER JOIN Response ON Response.request_id =" + primaryKey + "  AND Response.request_id = Request.dbid";
            while (isRowExist)
            {
                if (AnalyzerManager.ProxydbObj.CheckRowExist(rowCheck))
                {
                    isRowExist = false;
                }
            }

            DataTable dt = AnalyzerManager.ProxydbObj.GetTableFromDB(rowCheck, "PacketDetails");
            if (dt == null || dt.Rows.Count <= 0)
            {
                return;
            }
            Queue<SessionHandler> tempQue = PacketCreator.CreatePackets(dt);
            SessionHandler oSession = tempQue.FirstOrDefault();
            if (oSession != null)
            {
                AlertsProcessor.ProcessPacket(oSession);
            }
        }
    }

    class HookDllMessageProcessor
    {
        public static void ProcessMessage(byte[] data)
        {
            if (data != null && data.Length > 100 && data[0] == 0x02)
            {
                //DNS PAcket
                DNS_FULL_PACKET packet = new DNS_FULL_PACKET();

                object refp = (object)packet;

                PipeStructConverter.ByteArrayToDNS(data, ref refp);

                DNS_FULL_PACKET p = (DNS_FULL_PACKET)refp;

                AnalyzerManager.ProxydbObj.InsertDnsDetails(DateTime.UtcNow, p.dnsPacket.procName, (ushort)p.dnsPacket.pId, p.dnsPacket.dnsString,
                                         p.dnsPacket.filePath);
            }
            else if (data != null && data.Length > 100 && data[0] == 0x03)
            {
                FileCreation_FULL_PACKET packet = new FileCreation_FULL_PACKET();

                object refp = (object)packet;

                PipeStructConverter.ByteArrayToDNS(data, ref refp);

                FileCreation_FULL_PACKET p = (FileCreation_FULL_PACKET)refp;


                AnalyzerManager.ProxydbObj.InsertFileCreationDetails(DateTime.UtcNow, p.filePacket.filePath, p.filePacket.fileType, p.filePacket.isSuccess,
                                         p.filePacket.md5, p.filePacket.signature, p.filePacket.version);
            }
        }
    }
}
