using System;
using System.ServiceProcess;

namespace AddRegisterEntriesInstaller
{
    class Servicestarter
    {
        internal static void Start(RegistryEditor regEditor)
        {
            ////regEditor.Logger.Info("Enter");
            ServiceProxyServiceStart(regEditor);
            ServiceDataAnalyzerStart(regEditor);
            ////regEditor.Logger.Info("Exit");        
        }

        private static void ServiceProxyServiceStart(RegistryEditor regEditor)
        {
            ////regEditor.Logger.Info("Enter");

            try
            {
                using (ServiceController sc = new ServiceController("ProxyService"))
                {
                    if (sc.Status == ServiceControllerStatus.Stopped)
                    {
                        sc.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                //regEditor.Logger.Error(ex);
            }

            ////regEditor.Logger.Info("Exit");
        }

        private static void ServiceDataAnalyzerStart(RegistryEditor regEditor)
        {
            ////regEditor.Logger.Info("Enter");

            try
            {
                using (ServiceController sc = new ServiceController("DataAnalyzer"))
                {

                    if (sc.Status == ServiceControllerStatus.Stopped)
                    {
                        sc.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                //regEditor.Logger.Error(ex);
            }

            ////regEditor.Logger.Info("Exit");
        }
    }
}
