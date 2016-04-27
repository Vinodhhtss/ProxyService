using System;
using System.ServiceProcess;

namespace ProxyServiceAppln
{
    static class Program
    {
        static void Main()
        {
            var serviceToRun = new ProxyWindowsService();
            if (Environment.UserInteractive)
            {
                serviceToRun.Start();
                Console.ReadLine();
                serviceToRun.Stop();
            }
            else
            {
                ServiceBase[] ServicesToRun = new ServiceBase[]
                                                {
                                                    serviceToRun
                                                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
