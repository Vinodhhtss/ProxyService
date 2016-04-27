using HTTPDataAnalyzer.Registration;
using System;
using System.Data.SQLite;

namespace HTTPDataAnalyzer
{
    class UpdateQuery
    {
        public static void UpdateInHostInfo(SystemConfiguration sysconfig)
        {
            int identity = -1;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (SQLiteCommand identityCmd = new SQLiteCommand(connection))
                            {
                                identityCmd.CommandText = "SELECT max(dbid) FROM hostinfo";
                                object i = identityCmd.ExecuteScalar();

                                if (i != System.DBNull.Value)
                                {
                                    identity = Convert.ToInt32(i.ToString());
                                }

                                if (identity == -1)
                                {
                                    return;
                                }
                            }

                            using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                            {
                                insertSQL.CommandText = @"UPDATE hostinfo SET "
                                                                    + "hostname = @phostname,"
                                                                    + "agentversion = @pagentversion ,"
                                                                    + "timezone = @ptimezone ,"
                                                                    + "bitlevel = @pbitlevel,"
                                                                    + "osedition = @posedition,"
                                                                    + "osservicepack = @posservicepack ,"
                                                                    + "osname =  @posname,"
                                                                    + "oslastuptime=  @poslastuptime,"
                                                                    + "domainname = @pdomainname,"
                                                                    + "installdate = @pinstalldate,"
                                                                    + "productid = @pproductid ,"
                                                                    + "processor= @pprocessor,"
                                                                    + "primaryuser =  @pprimaryuser,"
                                                                    + "registereduser = @pregistereduser,"
                                                                    + "acrobat = @pacrobat ,"
                                                                    + "java = @pjava,"
                                                                    + "flash = @pflash,"
                                                                    + "chasistype = @pchasistype where dbid=@phostinfoid";

                                insertSQL.Parameters.AddWithValue("@phostname", sysconfig.HostName);
                                insertSQL.Parameters.AddWithValue("@pagentversion", sysconfig.AgentVersion);
                                insertSQL.Parameters.AddWithValue("@ptimezone", sysconfig.TimeZone);
                                insertSQL.Parameters.AddWithValue("@pbitlevel", sysconfig.BitLevel);
                                insertSQL.Parameters.AddWithValue("@posedition", sysconfig.OSEdition);
                                insertSQL.Parameters.AddWithValue("@posservicepack", sysconfig.OSServicePack);
                                insertSQL.Parameters.AddWithValue("@posname", sysconfig.OSName);
                                insertSQL.Parameters.AddWithValue("@poslastuptime", sysconfig.OSLastUpTime);
                                insertSQL.Parameters.AddWithValue("@pdomainname", sysconfig.DomainName);
                                insertSQL.Parameters.AddWithValue("@pinstalldate", sysconfig.Installdate);
                                insertSQL.Parameters.AddWithValue("@pproductid", sysconfig.ProductID);
                                insertSQL.Parameters.AddWithValue("@pprocessor", sysconfig.Processor);
                                insertSQL.Parameters.AddWithValue("@pprimaryuser", sysconfig.Primaryuser);
                                insertSQL.Parameters.AddWithValue("@pregistereduser", sysconfig.Registereduser);
                                insertSQL.Parameters.AddWithValue("@pacrobat", sysconfig.Acrobat);
                                insertSQL.Parameters.AddWithValue("@pjava", sysconfig.Java);
                                insertSQL.Parameters.AddWithValue("@pflash", sysconfig.Flash);
                                insertSQL.Parameters.AddWithValue("@pchasistype", sysconfig.ChasisType);
                                insertSQL.Parameters.AddWithValue("@phostinfoid", identity);

                                insertSQL.ExecuteNonQuery();
                                InsertQueries.InsertInNetworkType(sysconfig.NetworkTypes, identity, connection, true);
                                InsertQueries.InsertInBrowser(sysconfig.Browsers, identity, connection, true);
                                InsertQueries.InsertInOfficeApplication(sysconfig.OfficeApplications, identity, connection, true);
                                if (sysconfig.Autorunpoints != null && sysconfig.Autorunpoints.Count > 0)
                                {
                                    InsertQueries.InsertAutorunpointsDetails(sysconfig.Autorunpoints, identity, connection, true);
                                }

                                if (sysconfig.RecentApps != null && sysconfig.RecentApps.Count > 0)
                                {
                                    InsertQueries.InsertRecentlyInstall(sysconfig.RecentApps, identity, connection);
                                }
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
            catch (Exception ex)
            {
                //Registration.ClientRegistrar.Logger.Error(ex);
            }
        }
    }
}