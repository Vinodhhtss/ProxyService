using HTTPDataAnalyzer.Registration;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace HTTPDataAnalyzer
{
    public class InsertQueries
    {
        public static void InsertInHostInfo(SystemConfiguration sysconfig)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                            {
                                insertSQL.CommandText = @"INSERT INTO hostinfo("
                                                                    + "hostname,"
                                                                    + "agentid,"
                                                                    + "agentversion,"
                                                                    + "timezone,"
                                                                    + "bitlevel,"
                                                                    + "osedition,"
                                                                    + "osservicepack,"
                                                                    + "osname,"
                                                                    + "oslastuptime,"
                                                                    + "domainname,"
                                                                    + "installdate,"
                                                                    + "productid,"
                                                                    + "processor,"
                                                                    + "primaryuser,"
                                                                    + "registereduser,"
                                                                    + "acrobat,"
                                                                    + "java,"
                                                                    + "flash,"
                                                                    + "chasistype"
                                                                    + ")VALUES(@phostname,@pagentid,@pagentversion,@ptimezone,@pbitlevel,@posedition,@posservicepack,@posname"
                                                                    + ",@poslastuptime,@pdomainname,@pinstalldate,@pproductid,@pprocessor,@pprimaryuser,@pregistereduser"
                                                                    + ",@pacrobat,@pjava,@pflash,@pchasistype)";

                                insertSQL.Parameters.AddWithValue("@phostname", sysconfig.HostName);
                                insertSQL.Parameters.AddWithValue("@pagentid", sysconfig.AgentID);
                                insertSQL.Parameters.AddWithValue("@pagentversion", sysconfig.AgentVersion);
                                insertSQL.Parameters.AddWithValue("@ptimezone", sysconfig.TimeZone);
                                insertSQL.Parameters.AddWithValue("@pbitlevel", sysconfig.BitLevel);
                                insertSQL.Parameters.AddWithValue("@posedition", sysconfig.OSEdition);
                                insertSQL.Parameters.AddWithValue("@posservicepack", sysconfig.OSServicePack);
                                insertSQL.Parameters.AddWithValue("@posname", sysconfig.OSName);
                                insertSQL.Parameters.AddWithValue("@poslastuptime", sysconfig.OSLastUpTime);
                                insertSQL.Parameters.AddWithValue("@pdomainname", sysconfig.DomainName);
                                insertSQL.Parameters.AddWithValue("@pinstalldate", sysconfig.Installdate.ToString(DBManager.DateTimeFormat));
                                insertSQL.Parameters.AddWithValue("@pproductid", sysconfig.ProductID);
                                insertSQL.Parameters.AddWithValue("@pprocessor", sysconfig.Processor);
                                insertSQL.Parameters.AddWithValue("@pprimaryuser", sysconfig.Primaryuser);
                                insertSQL.Parameters.AddWithValue("@pregistereduser", sysconfig.Registereduser);
                                insertSQL.Parameters.AddWithValue("@pacrobat", sysconfig.Acrobat);
                                insertSQL.Parameters.AddWithValue("@pjava", sysconfig.Java);
                                insertSQL.Parameters.AddWithValue("@pflash", sysconfig.Flash);
                                insertSQL.Parameters.AddWithValue("@pchasistype", sysconfig.ChasisType);

                                insertSQL.ExecuteNonQuery();
                                int identity = (int)connection.LastInsertRowId;
                                InsertInNetworkType(sysconfig.NetworkTypes, identity, connection, false);
                                InsertInBrowser(sysconfig.Browsers, identity, connection, false);
                                InsertInOfficeApplication(sysconfig.OfficeApplications, identity, connection, false);
                                InsertAutorunpointsDetails(sysconfig.Autorunpoints, identity, connection, false);
                                InsertRecentlyInstall(sysconfig.RecentApps, identity, connection);
                            }
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void InsertInNetworkType(List<NetworkType> networktype, int hostinfoid, SQLiteConnection connection, bool isUpdate)
        {
            try
            {
                if (isUpdate)
                {
                    DeleteRows("networktype", "  WHERE hostinfoid= " + hostinfoid, connection);
                }

                using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                {
                    insertSQL.CommandText = @"INSERT INTO networktype("
                                                       + "hostinfoid,"
                                                       + "ipaddress,"
                                                       + "type,"
                                                       + "mac"
                                                       + ")VALUES(@phostinfoid,@pipaddress,@ptype,@pmac)";

                    insertSQL.Parameters.AddWithValue("@phostinfoid", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pipaddress", string.Empty);
                    insertSQL.Parameters.AddWithValue("@ptype", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pmac", string.Empty);
                    foreach (var item in networktype)
                    {
                        insertSQL.Parameters["@phostinfoid"].Value = hostinfoid;
                        insertSQL.Parameters["@pipaddress"].Value = item.IPAdress;
                        insertSQL.Parameters["@ptype"].Value = item.Type;
                        insertSQL.Parameters["@pmac"].Value = item.MAC;
                        insertSQL.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void InsertInBrowser(List<Browser> browser, int hostinfoid, SQLiteConnection connection, bool isUpdate)
        {
            try
            {
                if (isUpdate)
                {
                    DeleteRows("browser", "  WHERE hostinfoid= " + hostinfoid, connection);
                }

                using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                {
                    insertSQL.CommandText = @"INSERT INTO browser("
                                                        + "hostinfoid,"
                                                        + "name,"
                                                        + "version,"
                                                        + "proxyenabled"
                                                        + ")VALUES(@phostinfoid,@pname,@pversion,@pproxyenabled)";


                    insertSQL.Parameters.AddWithValue("@phostinfoid", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pname", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pversion", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pproxyenabled", string.Empty);
                    foreach (var item in browser)
                    {
                        insertSQL.Parameters["@phostinfoid"].Value = hostinfoid;
                        insertSQL.Parameters["@pname"].Value = item.Name;
                        insertSQL.Parameters["@pversion"].Value = item.Version;
                        insertSQL.Parameters["@pproxyenabled"].Value = item.ProxyEnabled;

                        insertSQL.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void InsertInOfficeApplication(List<OfficeApplication> officeapp, int hostinfoid, SQLiteConnection connection, bool isUpdate)
        {
            try
            {
                if (isUpdate)
                {
                    DeleteRows("officeapplication", "  WHERE hostinfoid= " + hostinfoid, connection);
                }
                using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                {

                    insertSQL.CommandText = @"INSERT INTO officeapplication("
                                                            + "hostinfoid,"
                                                            + "name,"
                                                            + "version"
                                                            + ")VALUES(@phostinfoid,@pname,@pversion)";

                    insertSQL.Parameters.AddWithValue("@phostinfoid", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pname", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pversion", string.Empty);

                    foreach (var item in officeapp)
                    {
                        insertSQL.Parameters["@phostinfoid"].Value = hostinfoid;
                        insertSQL.Parameters["@pname"].Value = item.Name;
                        insertSQL.Parameters["@pversion"].Value = item.Version;

                        insertSQL.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void InsertAutorunpointsDetails(List<HTTPDataAnalyzer.Registration.RecentAutoRuns> autorunpoints, int hostinfoid, SQLiteConnection connection, bool isUpdate)
        {
            try
            {
                if (isUpdate)
                {
                    DeleteRows("autorunpoints", "  WHERE hostinfoid= " + hostinfoid, connection);
                }
                using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                {
                    insertSQL.CommandText = @"INSERT INTO autorunpoints(hostinfoid,"
                                                                + "type,"
                                                                + "registrypath,"
                                                                + "filepath,"
                                                                + "isregistry,"
                                                                + "isfile,"
                                                                + "registrymodified,"
                                                                + "registryowner,"
                                                                + "registryvaluename,"
                                                                + "registryvaluestring,"
                                                                + "filecreated,"
                                                                + "filemodified,"
                                                                + "fileowner,"
                                                                + "filemd5,"
                                                                + "issigned,"
                                                                + "isverified,"
                                                                + "signaturestring,"
                                                                + "ca,"
                                                                + "certsubject,"
                                                                + "isdeleted,"
                                                                 + "collecteddate"
                                                                + ") VALUES(@phostinfoid,@ptype,@pregistrypath,@pfilepath,@pisregistry,@pisfile,"
                                                                + "@pregistrymodified,@pregistryowner,@pregistryvaluename,@pregistryvaluestring,@pfilecreated"
                                                                + ",@pfilemodified,@pfileowner,@pfilemd5,@pissigned,@pisverified,@psignaturestring,@pca,@pcertsubject,@pisdeleted,@pcollecteddate)";

                    insertSQL.Parameters.AddWithValue("@phostinfoid", string.Empty);
                    insertSQL.Parameters.AddWithValue("@ptype", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pregistrypath", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pfilepath", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pisregistry", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pisfile", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pregistrymodified", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pregistryowner", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pregistryvaluename", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pregistryvaluestring", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pfilecreated", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pfilemodified", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pfileowner", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pfilemd5", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pissigned", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pisverified", string.Empty);
                    insertSQL.Parameters.AddWithValue("@psignaturestring", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pca", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pcertsubject", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pisdeleted", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pcollecteddate", string.Empty);

                    foreach (var runs in autorunpoints)
                    {
                        if (!runs.IsDeleted)
                        {
                            var item = runs.AppDetails;
                            insertSQL.Parameters["@phostinfoid"].Value = hostinfoid;
                            insertSQL.Parameters["@ptype"].Value = item.Type;
                            insertSQL.Parameters["@pregistrypath"].Value = item.RegistryPath;
                            insertSQL.Parameters["@pfilepath"].Value = item.FilePath;
                            insertSQL.Parameters["@pisregistry"].Value = item.IsRegistry;
                            insertSQL.Parameters["@pisfile"].Value = item.IsFile;
                            insertSQL.Parameters["@pregistrymodified"].Value = item.RegistryModified;
                            insertSQL.Parameters["@pregistryowner"].Value = item.RegistryOwner;
                            insertSQL.Parameters["@pregistryvaluename"].Value = item.RegistryValueName;
                            insertSQL.Parameters["@pregistryvaluestring"].Value = item.RegistryValueString;
                            insertSQL.Parameters["@pfilecreated"].Value = item.FileCreated;
                            insertSQL.Parameters["@pfilemodified"].Value = item.FileModified;
                            insertSQL.Parameters["@pfileowner"].Value = item.FileOwner;
                            insertSQL.Parameters["@pfilemd5"].Value = item.FileMD5;
                            insertSQL.Parameters["@pissigned"].Value = item.IsSigned;
                            insertSQL.Parameters["@pisverified"].Value = item.IsVerified;
                            insertSQL.Parameters["@psignaturestring"].Value = item.SignatureString;
                            insertSQL.Parameters["@pca"].Value = item.CA;
                            insertSQL.Parameters["@pcertsubject"].Value = item.CertSubject;
                            insertSQL.Parameters["@pisdeleted"].Value = runs.IsDeleted;
                            insertSQL.Parameters["@pcollecteddate"].Value = runs.CollectedDate.ToString(DBManager.DateTimeFormat);
                        }
                        else
                        {
                           // if(string.IsNullOrEmpty(runs.AppDetails.FilePath))
                            {

                               // DeleteRows("autorunpoints", "  WHERE registrypath = " + "'" + runs.AppDetails.FilePath + "' AND registryvaluename = " + "'" + runs.AppDetails.RegistryValueName + "'" + "' AND registryvaluestring = " + "'" + runs.AppDetails.RegistryValueString + "'", connection);
                            }
                            //else
                            //{
                            //    DeleteRows("autorunpoints", "  WHERE filepath = " + "'" + runs.AppDetails.FilePath + "'", connection);
                            //}
                        }
                        insertSQL.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void InsertRecentlyInstall(List<RecentApp> reginstall, int hostinfoid, SQLiteConnection connection)
        {
            try
            {
                using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                {
                    insertSQL.CommandText = @"INSERT INTO installedapp(hostinfoid,"
                                                     + "displayname,"
                                                     + "version,"
                                                     + "installdate,"
                                                     + "key,"
                                                     + "is64,"
                                                     + "isinstalled,"
                                                     + "collecteddate"
                                                     + ") VALUES(@phostinfoid,@pdisplayname,@pversion,@pinstalldate,@pkey,@pis64,@pisinstalled,@pcollecteddate)";

                    insertSQL.Parameters.AddWithValue("@phostinfoid", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pdisplayname", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pversion", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pinstalldate", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pkey", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pis64", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pisinstalled", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pcollecteddate", string.Empty);

                    foreach (var recent in reginstall)
                    {
                        winaudits.InstalledApp item = recent.AppDetails;
                        if (recent.IsInstalled)
                        {
                            insertSQL.Parameters["@phostinfoid"].Value = hostinfoid;
                            insertSQL.Parameters["@pdisplayname"].Value = item.DisplayName;
                            insertSQL.Parameters["@pversion"].Value = item.Version;
                            insertSQL.Parameters["@pinstalldate"].Value = item.InstallDate;
                            insertSQL.Parameters["@pkey"].Value = item.Key;
                            insertSQL.Parameters["@pis64"].Value = item.Is64;
                            insertSQL.Parameters["@pisinstalled"].Value = recent.IsInstalled;
                            insertSQL.Parameters["@pcollecteddate"].Value = recent.CollectedDate;
                            insertSQL.ExecuteNonQuery();
                        }
                        else
                        {
                            DeleteRows("installedapp", "  WHERE key = " + "'" +item.Key + "'", connection);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void DeleteRows(string tableName, string condition, SQLiteConnection conn)
        {
            string query = "DELETE FROM " + tableName;
            if (condition != string.Empty)
            {
                query += condition;
            }

            try
            {
                using (SQLiteCommand command = new SQLiteCommand(query, conn))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}