using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace winaudits
{
    public class ReadQueries
    {
        public static AuditMaster GetAuditMaster(string tableName, int status)
        {
            DataTable dt = null;
            AuditMaster audit = null;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (SQLiteCommand selectCommand = new SQLiteCommand("select dbid, auditjobidserver,includeuser,includeprocess, " +
                           "includenetworkinfo,includeautorunpoints,includeprefetch," +
                           "includeservices,includedns,includearp,includeinstalledapp,includetask,status,receivedtime FROM " + tableName + " where status = " + status, connection))
                        {
                            using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                            {
                                if (tableName == string.Empty)
                                {
                                    tableName = "unknown";
                                }
                                dt = new DataTable(tableName);
                                dt.Load(dataReader);
                            }
                        }

                        string json = JsonConvert.SerializeObject(dt);

                        List<AuditMaster> listAuditMaster = JsonConvert.DeserializeObject<List<AuditMaster>>(json);

                        audit = listAuditMaster.FirstOrDefault();
                        if (audit != null)
                        {
                            UpdateQuery.UpdateAuditInitiateStatus(connection, audit.ClientJobID);
                        }
                        transaction.Commit();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return audit;
        }

        public static DataTable RunSelectQuery(string tableName, int auditjobidserver)
        {
            DataTable dt = null;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand("select * from " + tableName + " where auditmasterid = " + auditjobidserver, connection))
                    {
                        using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                        {
                            if (tableName == string.Empty)
                            {
                                tableName = "unknown";
                            }
                            dt = new DataTable(tableName);
                            dt.Load(dataReader);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return dt;
        }

        public static DataTable GetProcessModules(string tableName, int id)
        {
            DataTable dt = null;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand("select * from " + tableName + " where processtableid = " + id, connection))
                    {
                        using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                        {
                            if (tableName == string.Empty)
                            {
                                tableName = "unknown";
                            }
                            dt = new DataTable(tableName);
                            dt.Load(dataReader);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return dt;
        }

        public static DataTable GetAuditMasterByStatus(string tableName, int status)
        {
            DataTable dt = null;
            string tempCondition;
            if (status == 2)
            {
                tempCondition = " where status = " + status + " OR " + "status = " + 3;
            }
            else
            {
                tempCondition = " where status = " + status + " OR " + "status = " + 0;
            }

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand("select * from " + tableName + tempCondition, connection))
                    {
                        using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                        {
                            if (tableName == string.Empty)
                            {
                                tableName = "unknown";
                            }
                            dt = new DataTable(tableName);
                            dt.Load(dataReader);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return dt;
        }

        public static DataTable GetSentAudit(string tableName, int status)
        {
            DataTable dt = null;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand("select dbid from " + tableName + " where status = " + status, connection))
                    {
                        using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                        {
                            if (tableName == string.Empty)
                            {
                                tableName = "unknown";
                            }
                            dt = new DataTable(tableName);
                            dt.Load(dataReader);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return dt;
        }

        public static List<winaudits.Prefetch> GetPrefetch()
        {
            List<winaudits.Prefetch> prefetches = new List<Prefetch>();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand("select * from prefetch", connection))
                    {
                        using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                winaudits.Prefetch pref = new winaudits.Prefetch();
                                pref.FileName = dataReader["filename"].ToString();
                                pref.FullPath = dataReader["fullpath"].ToString();
                                pref.LastRun = dataReader["lastrun"].ToString();
                                pref.Created = dataReader["created"].ToString();
                                pref.TimesRun = Convert.ToInt32(dataReader["timesrun"].ToString());
                                pref.Size = Convert.ToInt32(dataReader["size"].ToString());
                                pref.HashOfPrefetch = dataReader["hashofprefetch"].ToString();

                                int prefechID = Convert.ToInt32(dataReader["dbid"].ToString());
                                pref.PrefetchPath = GetPrefetchPaths(prefechID, connection);
                                prefetches.Add(pref);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return prefetches;
        }
        
        private static List<string> GetPrefetchPaths(int prefechID, SQLiteConnection connection)
        {
            List<string> prefetches = new List<string>();
            try
            {
                using (SQLiteCommand selectCommand = new SQLiteCommand("select prefetchpath from prefetchpaths where prefetchid = " + prefechID, connection))
                {
                    using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            prefetches.Add(dataReader["prefetchpath"].ToString());
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return prefetches;
        }

        public static DataTable GetFileFetchAudit()
        {
            DataTable dt = null;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand("select auditjobid, filepath from filefetchaudit where status = 0", connection))
                    {
                        using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                        {
                            dt = new DataTable();
                            dt.Load(dataReader);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return dt;
        }
        
        public static DataTable GetRegistryFetchAudit()
        {
            DataTable dt = null;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand("select auditjobid, registrypath, registryhive from registryfetchaudit where status = 0", connection))
                    {
                        using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                        {
                            dt = new DataTable();
                            dt.Load(dataReader);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return dt;
        }
    }
}
