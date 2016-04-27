namespace HTTPDataAnalyzer.FailHandler
{
    class LazyFailHandler
    {
        public static void InsertInLazyFailed(byte[] input)
        {
            AnalyzerManager.ProxydbObj.InsertInLazyFailed(input);
        }
    }
}
