using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace HTTPDataAnalyzer
{
    public class ReadQueries
    {
        public static DataTable RunSelectQuery(string tableName)
        {
            DataTable dt = null;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand("select * from " + tableName, connection))
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


        public static DataTable GetFieldFromTable(string columnName, string tableName)
        {
            DataTable dt = null;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand("select " + columnName + " from " + tableName, connection))
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



        public static Dictionary<string, HTTPDataAnalyzer.Registration.RecentApp> GetPreviousConfig()
        {
            //Registration.ClientRegistrar.Logger.Error("Start Reading previous configuration");
            Dictionary<string, HTTPDataAnalyzer.Registration.RecentApp> oldApps = new Dictionary<string, Registration.RecentApp>();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand("select * from installedapp", connection))
                    {
                        using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                HTTPDataAnalyzer.Registration.RecentApp app = new Registration.RecentApp();
                                app.AppDetails = new winaudits.InstalledApp();
                                app.AppDetails.DisplayName = dataReader["displayname"].ToString();
                                app.AppDetails.Key = dataReader["key"].ToString();
                                app.AppDetails.InstallDate = dataReader["installdate"].ToString();
                                app.AppDetails.Version = dataReader["version"].ToString();
                                app.AppDetails.Is64 = Convert.ToBoolean(dataReader["is64"].ToString());
                                app.IsInstalled = Convert.ToBoolean(dataReader["isinstalled"].ToString());
                                app.CollectedDate = DateTime.Parse(dataReader["collecteddate"].ToString()).ToString(DBManager.DateTimeFormat);
                                if (!oldApps.ContainsKey(app.AppDetails.Key))
                                {
                                    oldApps.Add(app.AppDetails.Key, app);
                                }
                                else
                                {
                                    //Registration.ClientRegistrar.Logger.Error("=====================================================================");
                                    //Registration.ClientRegistrar.Logger.Error("Config 1");
                                    //Registration.ClientRegistrar.Logger.Error(app.AppDetails.DisplayName);
                                    //Registration.ClientRegistrar.Logger.Error(app.AppDetails.Key);
                                    //Registration.ClientRegistrar.Logger.Error(app.AppDetails.InstallDate);
                                    //Registration.ClientRegistrar.Logger.Error(app.AppDetails.Version);
                                    //Registration.ClientRegistrar.Logger.Error(app.AppDetails.Is64);
                                    //Registration.ClientRegistrar.Logger.Error(app.IsInstalled);
                                    //Registration.ClientRegistrar.Logger.Error(app.CollectedDate);

                                    app = oldApps[app.AppDetails.Key];
                                    //Registration.ClientRegistrar.Logger.Error("Config 2");
                                    //Registration.ClientRegistrar.Logger.Error(app.AppDetails.DisplayName);
                                    //Registration.ClientRegistrar.Logger.Error(app.AppDetails.Key);
                                    //Registration.ClientRegistrar.Logger.Error(app.AppDetails.InstallDate);
                                    //Registration.ClientRegistrar.Logger.Error(app.AppDetails.Version);
                                    //Registration.ClientRegistrar.Logger.Error(app.AppDetails.Is64);
                                    //Registration.ClientRegistrar.Logger.Error(app.IsInstalled);
                                    //Registration.ClientRegistrar.Logger.Error(app.CollectedDate);
                                    //Registration.ClientRegistrar.Logger.Error("=====================================================================");
                                }
                            }
                        }
                    }
                }

                //Registration.ClientRegistrar.Logger.Error("End Reading previous configuration");
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
            }
            return oldApps;
        }



        public static Dictionary<string, HTTPDataAnalyzer.Registration.RecentAutoRuns> GetPreviousAutoRuns()
        {
            Dictionary<string, HTTPDataAnalyzer.Registration.RecentAutoRuns> oldApps = new Dictionary<string, Registration.RecentAutoRuns>();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand("select * from autorunpoints", connection))
                    {
                        using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                HTTPDataAnalyzer.Registration.RecentAutoRuns autoRun = new Registration.RecentAutoRuns();
                                autoRun.AppDetails = new winaudits.Autorunpoints();
                                autoRun.AppDetails.Type = dataReader["type"].ToString();
                                autoRun.AppDetails.RegistryPath = dataReader["registrypath"].ToString();
                                autoRun.AppDetails.FilePath = dataReader["filepath"].ToString();
                                autoRun.AppDetails.IsRegistry = Convert.ToBoolean(dataReader["isregistry"].ToString());
                                autoRun.AppDetails.IsFile = Convert.ToBoolean(dataReader["isfile"].ToString());
                                autoRun.AppDetails.RegistryModified = dataReader["registrymodified"].ToString();
                                autoRun.AppDetails.RegistryOwner = dataReader["registryowner"].ToString();
                                autoRun.AppDetails.RegistryValueName = dataReader["registryvaluename"].ToString();
                                autoRun.AppDetails.RegistryValueString = dataReader["registryvaluestring"].ToString();
                                autoRun.AppDetails.FileCreated = dataReader["filecreated"].ToString();
                                autoRun.AppDetails.FileModified = dataReader["filemodified"].ToString();
                                autoRun.AppDetails.FileOwner = dataReader["fileowner"].ToString();
                                autoRun.AppDetails.FileMD5 = dataReader["filemd5"].ToString();
                                autoRun.AppDetails.IsSigned = Convert.ToBoolean(dataReader["issigned"].ToString());
                                autoRun.AppDetails.IsVerified = Convert.ToBoolean(dataReader["isverified"].ToString());
                                autoRun.AppDetails.SignatureString = dataReader["signaturestring"].ToString();
                                autoRun.AppDetails.CA = dataReader["ca"].ToString();
                                autoRun.AppDetails.CertSubject = dataReader["certsubject"].ToString();
                                autoRun.IsDeleted = Convert.ToBoolean(dataReader["isdeleted"].ToString());
                                autoRun.CollectedDate = DateTime.Parse(dataReader["collecteddate"].ToString());
                                oldApps.Add(autoRun.AppDetails.FilePath, autoRun);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
            }
            return oldApps;
        }

        public static HTTPDataAnalyzer.Registration.SystemConfiguration GetSystemConfiguration()
        {
            HTTPDataAnalyzer.Registration.SystemConfiguration sysConfig = null;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand("select * from hostinfo", connection))
                    {
                        using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                sysConfig = new Registration.SystemConfiguration();
                                HTTPDataAnalyzer.Registration.SystemConfiguration app = new Registration.SystemConfiguration();
                                app.HostName = dataReader["hostname"].ToString();
                                app.AgentVersion = dataReader["AgentVersion"].ToString();
                                app.TimeZone = dataReader["TimeZone"].ToString();
                                app.BitLevel = int.Parse(dataReader["BitLevel"].ToString());
                                app.OSEdition = dataReader["OSEdition"].ToString();
                                app.OSServicePack = dataReader["OSServicePack"].ToString();
                                app.OSName = dataReader["OSName"].ToString();
                                app.OSLastUpTime = DateTime.Parse(dataReader["OSLastUpTime"].ToString());
                                app.DomainName = dataReader["DomainName"].ToString();
                                app.Installdate = DateTime.Parse(dataReader["Installdate"].ToString());
                                app.ProductID = dataReader["ProductID"].ToString();
                                app.Processor = dataReader["Processor"].ToString();
                                app.Primaryuser = dataReader["Primaryuser"].ToString();
                                app.Registereduser = dataReader["Registereduser"].ToString();
                                app.Acrobat = dataReader["Acrobat"].ToString();
                                app.Java = dataReader["Java"].ToString();
                                app.Flash = dataReader["Flash"].ToString();
                                app.ChasisType = dataReader["ChasisType"].ToString();
                                app.Browsers = GetBrowser();
                                app.NetworkTypes = GetNetworkType();
                                app.OfficeApplications = GetOfficeApplication();
                                app.Autorunpoints = GetAutorunpoints();
                                app.RecentApps = GetRecentApp();
                                sysConfig = app;
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
            }
            return sysConfig;
        }

        public static List<HTTPDataAnalyzer.Registration.Browser> GetBrowser()
        {
            List<HTTPDataAnalyzer.Registration.Browser> oldApp = new List<Registration.Browser>();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand("select * from browser", connection))
                    {
                        using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                HTTPDataAnalyzer.Registration.Browser app = new Registration.Browser();
                                app.Name = dataReader["name"].ToString();
                                app.ProxyEnabled = int.Parse(dataReader["proxyenabled"].ToString());
                                app.Version = dataReader["version"].ToString();
                                oldApp.Add(app);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
            }
            return oldApp;
        }

        public static List<HTTPDataAnalyzer.Registration.NetworkType> GetNetworkType()
        {
            List<HTTPDataAnalyzer.Registration.NetworkType> oldApp = new List<Registration.NetworkType>();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand("select * from networktype", connection))
                    {
                        using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                HTTPDataAnalyzer.Registration.NetworkType app = new Registration.NetworkType();
                                app.IPAdress = dataReader["ipaddress"].ToString();
                                app.MAC = dataReader["mac"].ToString();
                                app.Type = dataReader["type"].ToString();
                                oldApp.Add(app);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
            }
            return oldApp;
        }

        public static List<HTTPDataAnalyzer.Registration.OfficeApplication> GetOfficeApplication()
        {
            List<HTTPDataAnalyzer.Registration.OfficeApplication> oldApp = new List<Registration.OfficeApplication>();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand("select * from officeapplication", connection))
                    {
                        using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                HTTPDataAnalyzer.Registration.OfficeApplication app = new Registration.OfficeApplication();
                                app.Name = dataReader["name"].ToString();
                                app.Version = dataReader["version"].ToString();

                                oldApp.Add(app);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex); 
            }
            return oldApp;
        }

        public static List<HTTPDataAnalyzer.Registration.RecentAutoRuns> GetAutorunpoints()
        {
            List<HTTPDataAnalyzer.Registration.RecentAutoRuns> oldApp = new List<Registration.RecentAutoRuns>();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand("select * from autorunpoints", connection))
                    {
                        using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                HTTPDataAnalyzer.Registration.RecentAutoRuns app = new Registration.RecentAutoRuns();

                                app.CollectedDate = DateTime.Parse(dataReader["collecteddate"].ToString());
                                app.IsDeleted = bool.Parse(dataReader["isdeleted"].ToString());
                                app.AppDetails = new winaudits.Autorunpoints();
                                app.AppDetails.Type = dataReader["type"].ToString();
                                app.AppDetails.RegistryPath = dataReader["registrypath"].ToString();
                                app.AppDetails.FilePath = dataReader["filepath"].ToString();
                                app.AppDetails.IsRegistry = Convert.ToBoolean(dataReader["isregistry"].ToString());
                                app.AppDetails.IsFile = Convert.ToBoolean(dataReader["isfile"].ToString());
                                app.AppDetails.RegistryModified = dataReader["registrymodified"].ToString();
                                app.AppDetails.RegistryOwner = dataReader["registryowner"].ToString();
                                app.AppDetails.RegistryValueName = dataReader["registryvaluename"].ToString();
                                app.AppDetails.RegistryValueString = dataReader["registryvaluestring"].ToString();
                                app.AppDetails.FileCreated = dataReader["filecreated"].ToString();
                                app.AppDetails.FileModified = dataReader["filemodified"].ToString();
                                app.AppDetails.FileOwner = dataReader["fileowner"].ToString();
                                app.AppDetails.FileMD5 = dataReader["filemd5"].ToString();
                                app.AppDetails.IsSigned = Convert.ToBoolean(dataReader["issigned"].ToString());
                                app.AppDetails.IsVerified = Convert.ToBoolean(dataReader["isverified"].ToString());
                                app.AppDetails.SignatureString = dataReader["signaturestring"].ToString();
                                app.AppDetails.CA = dataReader["ca"].ToString();
                                app.AppDetails.CertSubject = dataReader["certsubject"].ToString();

                                oldApp.Add(app);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
            }
            return oldApp;
        }

        public static List<HTTPDataAnalyzer.Registration.RecentApp> GetRecentApp()
        {
            List<HTTPDataAnalyzer.Registration.RecentApp> oldApp = new List<Registration.RecentApp>();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand("select * from installedapp", connection))
                    {
                        using (SQLiteDataReader dataReader = selectCommand.ExecuteReader())
                        {
                            List<winaudits.InstalledApp> autoRun = new List<winaudits.InstalledApp>();
                            while (dataReader.Read())
                            {
                                HTTPDataAnalyzer.Registration.RecentApp app = new Registration.RecentApp();

                                app.CollectedDate = dataReader["collecteddate"].ToString();
                                app.IsInstalled = bool.Parse(dataReader["isinstalled"].ToString());
                                app.AppDetails = new winaudits.InstalledApp();
                                app.AppDetails.DisplayName = dataReader["displayname"].ToString();
                                app.AppDetails.Version = dataReader["version"].ToString();
                                app.AppDetails.InstallDate = dataReader["installdate"].ToString();
                                app.AppDetails.Key = dataReader["key"].ToString();
                                app.AppDetails.Is64 = bool.Parse(dataReader["is64"].ToString());

                                oldApp.Add(app);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
            }
            return oldApp;
        }
    }
}
