//using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using winaudits;

namespace WinAudit
{
    class Program
    {
        static void Main()
        {
            //List<InstalledApp> recentinstall = InstalledAppAuditor.StartAudit();
            //winaudits.DBManager.Start();
            //List<Prefetch> recentinstall = PrefetchAuditor.StartAudit();
            //InsertQueries.InsertPrefetchDetails(recentinstall, 1);
            //////  //DriveInfo[] drives = DriveInfo.GetDrives();

            ////  //foreach (DriveInfo drive in drives)
            ////  //{
            ////  //    Console.WriteLine(drive.Name);
            ////  //    Console.WriteLine(drive.TotalSize);
            ////  //}

            ////  //winaudits.ReadQueries.GetPrefetch();
            //Console.ReadKey();
            //List<winaudits.Autorunpoints> runPoints = winaudits.AutoRunManager.StartAudit();
            //InsertQueries.InsertAutorunpointsDetails(runPoints, 1);
            //return;
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            //ILog tempLogger = CLogger.CreateLogger();
            //if (tempLogger != null)
            //{
            //    AuditManager.Logger = tempLogger;
            //}
            try
            {
                AuditManager.StartAudit();
            }
            catch (Exception ex)
            {
                //AuditManager.Logger.Error(ex);
            }
        }

        static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            //AuditManager.Logger.Error(e);
        }
    }
}
