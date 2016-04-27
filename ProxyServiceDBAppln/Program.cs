using System;
using System.ServiceProcess;

namespace ProxyServiceDBAppln
{
    static class Program
    {
        static void Main()
        {
            var analyzerService = new DataAnalyzer();
            if (Environment.UserInteractive)
            {
                analyzerService.Start();
                //Console.WriteLine("... press <ENTER> to quit");
                Console.ReadLine();
                analyzerService.Stop();
            }
            else
            {
                ServiceBase[] ServicesToRun = new ServiceBase[]
                                                {
                                                    analyzerService
                                                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
