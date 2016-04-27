using System.ServiceProcess;

namespace ProxyServiceDBAppln
{
    public partial class DataAnalyzer : ServiceBase
    {
        public DataAnalyzer()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Start();
        }

        internal void Start()
        {
            HTTPDataAnalyzer.AnalyzerManager.Analyzer = new HTTPDataAnalyzer.AnalyzerManager();
        }

        protected override void OnStop()
        {
            HTTPDataAnalyzer.AnalyzerManager.Analyzer.Stop();
        }
    }
}
