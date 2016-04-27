using Ionic.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using winaudits;

namespace HTTPDataAnalyzer
{
    class AuditProcessor
    {
        public const int ProcessExhaustTime = 900000;
        public static void ProcessAudit(string[] auditJob)
        {
            string[] auditID = auditJob[0].Split(new string[] { ": " }, 2, StringSplitOptions.RemoveEmptyEntries);
            int auditTypeID = int.Parse(auditID[1]);
            if (auditTypeID == 1)
            {
                StartWindAuditor(auditJob[1]);
            }
            else if (auditTypeID == 2)
            {
                winaudits.FileFetch audit = winaudits.FileFetch.GetFileFetch(auditJob[1]);
                winaudits.InsertQueries.InsertFileFetch(audit);
            }
            else if (auditTypeID == 3)
            {
                winaudits.RegistryFetch audit = winaudits.RegistryFetch.GetFileFetch(auditJob[1]);
                winaudits.InsertQueries.InsertRegFetch(audit);
            }
        }

        public static void StartWindAuditor(string auditMaster)
        {
            winaudits.AuditMaster audit = winaudits.AuditMaster.GetAuditMaster(auditMaster);
            winaudits.InsertQueries.InsertInAuditMaster(audit);
            StartWindAuditor();
        }

        public static void StartWindAuditor()
        {
            string programName = GetConfigFilePath();
            if (programName != string.Empty)
            {
                Process process = new Process();
                process.StartInfo.FileName = programName;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
            }
        }

        public static void ExistingAudit()
        {

            DataTable auditMasterTable = winaudits.ReadQueries.GetAuditMasterByStatus("auditmaster", 1);
            if (auditMasterTable != null && auditMasterTable.Rows.Count > 0)
            {
                string json = JsonConvert.SerializeObject(auditMasterTable);

                List<AuditMaster> listAuditMaster = JsonConvert.DeserializeObject<List<AuditMaster>>(json);
                foreach (AuditMaster auditMaster in listAuditMaster)
                {

                    DateTime dt = auditMaster.ReceivedTime ?? DateTime.Now;
                    if (dt != null)
                    {
                        TimeSpan timeDiff = DateTime.Now - dt;

                        if ((int)timeDiff.TotalMilliseconds > ProcessExhaustTime)
                        {
                            foreach (var process in Process.GetProcessesByName("WindAuditor"))
                            {
                                try
                                {
                                    process.Kill();
                                }
                                catch (Exception)
                                {
                                }
                            }
                            winaudits.UpdateQuery.ExcecuteUpdateQuery(3, auditMaster.ClientJobID);
                        }
                    }
                }
            }
            SendAuditResult();
            SendFileFetchAuditResult();
            SendRegistryFetchAuditResult();
            //Task.Factory.StartNew(SendAuditResult);
        }

        public static void SendAuditResult()
        {
            DataTable auditMasterTable = winaudits.ReadQueries.GetAuditMasterByStatus("auditmaster", 2);
            if (auditMasterTable != null && auditMasterTable.Rows.Count > 0)
            {
                string json = JsonConvert.SerializeObject(auditMasterTable);

                List<AuditMaster> listAuditMaster = JsonConvert.DeserializeObject<List<AuditMaster>>(json);

                foreach (AuditMaster auditMaster in listAuditMaster)
                {
                    if (auditMaster.Status == 2)
                    {
                        try
                        {
                            var zipStream = new MemoryStream();
                            var zip = new ZipOutputStream(zipStream);

                            if (auditMaster.IncludeUser == 1)
                            {
                                AddJsonEntry("user", auditMaster, zip);
                            }
                            if (auditMaster.IncludeProcess == 1)
                            {
                                AddJsonEntry("process", auditMaster, zip);
                            }
                            if (auditMaster.IncludeServices == 1)
                            {
                                AddJsonEntry("services", auditMaster, zip);
                            }
                            if (auditMaster.IncludeInstalledApp == 1)
                            {
                                AddJsonEntry("installedapp", auditMaster, zip);
                            }
                            if (auditMaster.IncludeNetworkInfo == 1)
                            {
                                AddJsonEntry("networkconnection", auditMaster, zip);
                            }
                            if (auditMaster.IncludeAutoRunPoints == 1)
                            {
                                AddJsonEntry("autorunpoints", auditMaster, zip);
                            }
                            if (auditMaster.IncludePrefetch == 1)
                            {
                                AddJsonEntry("prefetch", auditMaster, zip);
                            }
                            if (auditMaster.IncludeTask == 1)
                            {
                                AddJsonEntry("task", auditMaster, zip);
                            }
                            if (auditMaster.IncludeArp == 1)
                            {
                                AddJsonEntry("arp", auditMaster, zip);
                            }

                            zip.Close();
                            string encoding = Convert.ToBase64String(zipStream.ToArray());
                            if (TestTCPClient.TestConfig.TestCheck)
                            {
                                TestTCPClient.SendAuditResults("31", Encoding.ASCII.GetBytes(encoding), auditMaster.ServerJobID);
                            }
                            else
                            {
                                TCPClients.SendAuditResults("31", Encoding.ASCII.GetBytes(encoding), auditMaster.ServerJobID);
                            }
                            winaudits.UpdateQuery.ExcecuteUpdateQuery(4, auditMaster.ClientJobID);
                        }
                        catch (Exception)
                        {

                        }
                    }
                    else
                    {
                        if (TestTCPClient.TestConfig.TestCheck)
                        {
                            TestTCPClient.SendAuditResults("31", Encoding.ASCII.GetBytes(string.Empty), auditMaster.ServerJobID);
                        }
                        else
                        {
                            TCPClients.SendAuditResults("31", Encoding.ASCII.GetBytes(string.Empty), auditMaster.ServerJobID);
                        }
                        winaudits.UpdateQuery.ExcecuteUpdateQuery(4, auditMaster.ClientJobID);
                    }
                }
            }
        }

        public static void SendFileFetchAuditResult()
        {
            DataTable dt = winaudits.ReadQueries.GetFileFetchAudit();
            string json = JsonConvert.SerializeObject(dt);

            List<winaudits.FileFetch> listAuditMaster = JsonConvert.DeserializeObject<List<winaudits.FileFetch>>(json);

            foreach (var fileFetch in listAuditMaster)
            {
                if (fileFetch != null)
                {
                    winaudits.UpdateQuery.UpdateFileFetchAuditStatus(1, fileFetch.AuditJobID);
                    TCPClients.SendFileFetchResults("31", fileFetch);
                }
            }
        }

        public static void SendRegistryFetchAuditResult()
        {
            DataTable dt = winaudits.ReadQueries.GetRegistryFetchAudit();
            string json = JsonConvert.SerializeObject(dt);

            List<winaudits.RegistryFetch> listAuditMaster = JsonConvert.DeserializeObject<List<winaudits.RegistryFetch>>(json);

            foreach (var fetch in listAuditMaster)
            {
                if (fetch != null)
                {
                    winaudits.UpdateQuery.UpdateRegistryFetchAuditStatus(1, fetch.AuditJobID);
                    TCPClients.SendRegistryFetchResults("31", fetch);
                }
            }
        }

        private static void AddJsonEntry(string tableName, AuditMaster auditMaster, ZipOutputStream zip)
        {
            string json = null;
            if (tableName != "prefetch")
            {
                DataTable dt = winaudits.ReadQueries.RunSelectQuery(tableName, auditMaster.ClientJobID);
                try
                {
                    dt.PrimaryKey = null;
                    if (tableName != "process")
                    {
                        dt.Columns.Remove("dbid");
                    }
                    dt.Columns.Remove("auditmasterid");
                }
                catch (Exception)
                {
                }
                if (tableName == "process")
                {
                    json = JsonConvert.SerializeObject(dt);
                    List<RunningProcess> listRunningProcess = JsonConvert.DeserializeObject<List<RunningProcess>>(json);
                    foreach (var runProcess in listRunningProcess)
                    {
                        dt = winaudits.ReadQueries.GetProcessModules("modules", runProcess.ID);
                        json = JsonConvert.SerializeObject(dt);
                        runProcess.ListProcessModule = JsonConvert.DeserializeObject<List<LoadedModule>>(json);
                    }
                    json = JsonConvert.SerializeObject(listRunningProcess);
                }
                else
                {
                    json = JsonConvert.SerializeObject(dt);
                }
            }
            else
            {
                List<winaudits.Prefetch> prefeches = winaudits.ReadQueries.GetPrefetch();
                json = JsonConvert.SerializeObject(prefeches);
            }
            byte[] configByte = Encoding.ASCII.GetBytes(json);
            AddEntry(tableName + ".json", configByte, zip);
        }

        private static void AddEntry(string fileName, byte[] fileContent, ZipOutputStream zip)
        {
            zip.PutNextEntry(fileName);
            zip.Write(fileContent, 0, fileContent.Length);
        }

        private static string GetConfigFilePath()
        {
            string pathToCopyConfigFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            pathToCopyConfigFile = Path.Combine(pathToCopyConfigFile, "WindAuditor.exe");
            if (File.Exists(pathToCopyConfigFile))
            {
                return pathToCopyConfigFile;
            }

            return string.Empty;
        }
    }
}
