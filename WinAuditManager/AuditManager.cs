//using log4net;
using System.Collections.Generic;
using winaudits;

namespace WinAudit
{
    class AuditManager
    {
        //public static ILog Logger;
        internal static void StartAudit()
        {
            AuditMaster audit = winaudits.ReadQueries.GetAuditMaster("auditmaster", 0);
            if (audit != null)
            {
                winaudits.UpdateQuery.ExcecuteUpdateQuery(1, audit.ClientJobID);

                try
                {
                    if (audit.IncludeUser == 1)
                    {
                        List<User> users = UserProfileAuditor.StartAudit();
                        InsertQueries.InsertUserDetails(users, audit.ClientJobID);
                    }

                    if (audit.IncludeProcess == 1)
                    {
                        List<RunningProcess> process = ProcessList.StartAudit();
                        InsertQueries.InsertProcessDetails(process, audit.ClientJobID);
                    }

                    if (audit.IncludeServices == 1)
                    {
                        List<Services> service = ServiceEnum.StartAudit();
                        InsertQueries.InsertServicesDetails(service, audit.ClientJobID);
                    }

                    if (audit.IncludeNetworkInfo == 1)
                    {
                        List<winaudits.Networkconnection> networkconnection = winaudits.NetworkAuditor.StartAudit();
                        InsertQueries.InsertNetworkconnectionDetails(networkconnection, audit.ClientJobID);
                    }

                    if (audit.IncludeInstalledApp == 1)
                    {
                        List<InstalledApp> recentinstall = InstalledAppAuditor.StartAudit();
                        InsertQueries.InsertRecentlyInstall(recentinstall, audit.ClientJobID);
                    }

                    if (audit.IncludeAutoRunPoints == 1)
                    {
                        List<winaudits.Autorunpoints> runPoints = winaudits.AutoRunManager.StartAudit();
                        InsertQueries.InsertAutorunpointsDetails(runPoints, audit.ClientJobID);
                    }

                    if (audit.IncludePrefetch == 1)
                    {
                        List<Prefetch> recentinstall = PrefetchAuditor.StartAudit();
                        InsertQueries.InsertPrefetchDetails(recentinstall, audit.ClientJobID);
                    }

                    if (audit.IncludeTask == 1)
                    {
                        List<RunningTasks> runningtasks = TaskAuditor.StartAudit();
                        InsertQueries.InsertTaskDetails(runningtasks, audit.ClientJobID);
                    }

                    if (audit.IncludeArp == 1)
                    {
                        List<ARP> runningtasks = ARPAuditor.StartAudit();
                        InsertQueries.InsertArpDetails(runningtasks, audit.ClientJobID);
                    }
                }
                catch (System.Exception ex)
                {

                    //AuditManager.Logger.Error(ex);
                }

                winaudits.UpdateQuery.ExcecuteUpdateQuery(2, audit.ClientJobID);
            }
        }
    }
}
