using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTTPDataAnalyzer
{
    class PacketFilter
    {
        public static Queue<SessionHandler> RequestHeaderFilter(Queue<SessionHandler> oSessions, string headerAndValue)
        {
            Queue<SessionHandler> oSesHaler = new Queue<SessionHandler>();
            string[] headerAndValues = headerAndValue.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var oSession in oSessions)
            {
                if (oSession.RequestLines.ContainsKey(headerAndValues[0].ToUpper()))
                {
                    if (oSession.RequestLines[headerAndValues[0].ToUpper()].ToLower() == headerAndValues[1].ToLower())
                    {
                        continue;
                    }
                }
                oSesHaler.Enqueue(oSession);
            }

            return oSesHaler;
        }

        public static Queue<SessionHandler> ResponseHeaderFilter(Queue<SessionHandler> oSessions, string headerAndValue)
        {
            Queue<SessionHandler> oSesHaler = new Queue<SessionHandler>();
            string[] headerAndValues = headerAndValue.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var oSession in oSessions)
            {
                if (oSession.ResponseLines.ContainsKey(headerAndValues[0].ToUpper()))
                {
                    if (oSession.ResponseLines[headerAndValues[0].ToUpper()].ToLower() == headerAndValues[1].ToLower())
                    {
                        continue;
                    }
                }
                oSesHaler.Enqueue(oSession);
            }

            return oSesHaler;
        }
    }
}
