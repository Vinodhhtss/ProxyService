using HTTPProxyServer;
using System.ServiceProcess;

namespace ProxyServiceAppln
{
    public partial class ProxyWindowsService : ServiceBase
    {
        public ProxyWindowsService()
        {
            InitializeComponent();
            base.CanHandleSessionChangeEvent = true;
        }

        protected override void OnStart(string[] args)
        {
            Start();
        }
        
        protected override void OnStop()
        {
            ProxyServiceManager.Server.Stop();
        }

        internal void Start()
        {
            ProxyServiceManager.Server.Start();
        }

        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            switch (changeDescription.Reason)
            {
                case SessionChangeReason.SessionLogon:
                case SessionChangeReason.ConsoleConnect:
                    TCPClientProcessor.GetWindowsUserName();
                    break;
                default:
                    break;
            }

            base.OnSessionChange(changeDescription);
        }
    }
}
