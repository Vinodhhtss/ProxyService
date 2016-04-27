namespace HTTPDataAnalyzer
{
     interface IAlert
    {
        bool IsAlert(SessionHandler oSessionHandler);
        void SendAlert(SessionHandler oSessionHandler);
    }
}
