
namespace HTTPDataAnalyzer
{
    public class AlertsProcessor
    {
        public static void ProcessPacket(SessionHandler oSessionHandler)
        {
            IAlert alert1 = new AlertOne();
            if (alert1.IsAlert(oSessionHandler))
            {
                alert1.SendAlert(oSessionHandler);
            }
        }
    }
}
