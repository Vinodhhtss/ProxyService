namespace HTTPDataAnalyzer.FailHandler
{
    class AlertFailHandler
    {
        public static void InsertInAlertFailed(byte[] input)
        {
            AnalyzerManager.ProxydbObj.InsertInAlertFailed(input);
        }
    }
}
