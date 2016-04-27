using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;

namespace HTTPProxyServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //Temp Code
            AppDomain.CurrentDomain.FirstChanceException += FirstChanceHandler;
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            if (ProxyServiceManager.Server.Start())
            {
                Console.WriteLine(String.Format("Server started on {0}:{1}...Press enter key to end", ProxyServiceManager.Server.ListeningIPInterface, ProxyServiceManager.Server.ListeningPort));
                Console.ReadLine();
                Console.WriteLine("Shutting down");
                ProxyServiceManager.Server.Stop();
                Console.WriteLine("Server stopped...");
            }
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }


        //Temp Code
        static void FirstChanceHandler(object source, FirstChanceExceptionEventArgs e)
        {
           // TCPClientProcessor.Proxylog.WriteLogException(e.Exception);
        }

        static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
           // TCPClientProcessor.Proxylog.WriteLogException(e);
        }
    }
}
