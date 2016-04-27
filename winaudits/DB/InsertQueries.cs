using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace winaudits
{
    public class InsertQueries
    {
        public static void InsertInAuditMaster(AuditMaster auditMaster)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();

                    using (SQLiteCommand insertSQL = new SQLiteCommand(
                                                @"INSERT INTO auditmaster (dbid,"
                                                + "auditjobidserver,"
                                                + "includeuser,"
                                                + "includeprocess,"
                                                + "includenetworkinfo,"
                                                + "includeautorunpoints,"
                                                + "includeprefetch,"
                                                + "includeservices,"
                                                + "includedns,"
                                                + "includearp,"
                                                + "includeinstalledapp,"
                                                + "includetask,"
                                                + "status,"
                                                + "receivedtime"
                                                + ") VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?)",
                                                 connection))
                    {

                        SQLiteParameter param1 = new SQLiteParameter();
                        insertSQL.Parameters.Add(param1);

                        param1 = new SQLiteParameter();
                        param1.Value = auditMaster.ServerJobID;
                        insertSQL.Parameters.Add(param1);

                        param1 = new SQLiteParameter();
                        param1.Value = auditMaster.IncludeUser;
                        insertSQL.Parameters.Add(param1);

                        param1 = new SQLiteParameter();
                        param1.Value = auditMaster.IncludeProcess;
                        insertSQL.Parameters.Add(param1);

                        param1 = new SQLiteParameter();
                        param1.Value = auditMaster.IncludeNetworkInfo;
                        insertSQL.Parameters.Add(param1);

                        param1 = new SQLiteParameter();
                        param1.Value = auditMaster.IncludeAutoRunPoints;
                        insertSQL.Parameters.Add(param1);

                        param1 = new SQLiteParameter();
                        param1.Value = auditMaster.IncludePrefetch;
                        insertSQL.Parameters.Add(param1);

                        param1 = new SQLiteParameter();
                        param1.Value = auditMaster.IncludeServices;
                        insertSQL.Parameters.Add(param1);

                        param1 = new SQLiteParameter();
                        param1.Value = auditMaster.IncludeDns;
                        insertSQL.Parameters.Add(param1);

                        param1 = new SQLiteParameter();
                        param1.Value = auditMaster.IncludeArp;
                        insertSQL.Parameters.Add(param1);

                        param1 = new SQLiteParameter();
                        param1.Value = auditMaster.IncludeInstalledApp;
                        insertSQL.Parameters.Add(param1);

                        param1 = new SQLiteParameter();
                        param1.Value = auditMaster.IncludeTask;
                        insertSQL.Parameters.Add(param1);

                        param1 = new SQLiteParameter();
                        param1.Value = auditMaster.Status;
                        insertSQL.Parameters.Add(param1);

                        param1 = new SQLiteParameter();
                        param1.Value = DateTime.Now;
                        insertSQL.Parameters.Add(param1);

                        insertSQL.ExecuteNonQuery();
                        auditMaster.ClientJobID = (int)connection.LastInsertRowId;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void InsertUserDetails(List<User> users, int auditMasterId)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                    {
                        insertSQL.CommandText = @"INSERT INTO user (auditmasterid, username,"
                                           + "fullname, description,"
                                           + "sid, sidtype, lastlogin, disabled,"
                                           + "locked, passwordrequired,"
                                           + "passwordage, groups"
                                            + ") VALUES (@pauditmasterid, @pusername, @pfullname, @pdescription, @psid, @psidtype, @plastlogin,"
                                            + "@pdisabled, @plocked, @ppasswordrequired, @ppasswordage, @pgroups)";
                        {

                            insertSQL.Parameters.AddWithValue("@pauditmasterid", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pusername", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pfullname", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pdescription", string.Empty);
                            insertSQL.Parameters.AddWithValue("@psid", string.Empty);
                            insertSQL.Parameters.AddWithValue("@psidtype", string.Empty);
                            insertSQL.Parameters.AddWithValue("@plastlogin", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pdisabled", string.Empty);
                            insertSQL.Parameters.AddWithValue("@plocked", string.Empty);
                            insertSQL.Parameters.AddWithValue("@ppasswordrequired", string.Empty);
                            insertSQL.Parameters.AddWithValue("@ppasswordage", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pgroups", string.Empty);

                            foreach (var item in users)
                            {
                                insertSQL.Parameters["@pauditmasterid"].Value = auditMasterId;
                                insertSQL.Parameters["@pusername"].Value = item.UserName;
                                insertSQL.Parameters["@pfullname"].Value = item.FullName;
                                insertSQL.Parameters["@pdescription"].Value = item.Description;
                                insertSQL.Parameters["@psid"].Value = item.SID;
                                insertSQL.Parameters["@psidtype"].Value = item.SIDType;
                                insertSQL.Parameters["@plastlogin"].Value = item.LastLogin.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                insertSQL.Parameters["@pdisabled"].Value = item.IsDisabled;
                                insertSQL.Parameters["@plocked"].Value = item.IsLocked;
                                insertSQL.Parameters["@ppasswordrequired"].Value = item.PasswordRequired;
                                insertSQL.Parameters["@ppasswordage"].Value = item.PasswordAge;
                                insertSQL.Parameters["@pgroups"].Value = item.Groups;
                                insertSQL.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void InsertProcessDetails(List<RunningProcess> process, int auditMasterId)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                        {
                            insertSQL.CommandText = @"INSERT INTO process(auditmasterid, processname,"
                                               + "pid, path, arguments, starttime,"
                                               + "kerneltime, usertime, workingset,"
                                               + "username, ishidden, sid,"
                                               + "sidtype, parentname, ppid, md5, issigned, isverified, signaturestring, ca, certsubject)"
                                               + "VALUES(@pauditmasterid, @pprocessname,"
                                               + "@ppid, @ppath, @parguments, @pstarttime,"
                                               + "@pkerneltime, @pusertime, @pworkingset,"
                                               + "@pusername, @pishidden, @psid,"
                                               + "@psidtype, @pparentname, @ppid, @pmd5, @pissigned, @pisverified, @psignaturestring, @pca, @pcertsubject)";

                            insertSQL.Parameters.AddWithValue("@pauditmasterid", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pprocessname", string.Empty);
                            insertSQL.Parameters.AddWithValue("@ppid", string.Empty);
                            insertSQL.Parameters.AddWithValue("@ppath", string.Empty);
                            insertSQL.Parameters.AddWithValue("@parguments", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pstarttime", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pkerneltime", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pusertime", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pworkingset", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pusername", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pishidden", string.Empty);
                            insertSQL.Parameters.AddWithValue("@psid", string.Empty);
                            insertSQL.Parameters.AddWithValue("@psidtype", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pparentname", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pmd5", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pissigned", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pisverified", string.Empty);
                            insertSQL.Parameters.AddWithValue("@psignaturestring", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pca", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pcertsubject", string.Empty);

                            foreach (var item in process)
                            {
                                insertSQL.Parameters["@pauditmasterid"].Value = auditMasterId;
                                insertSQL.Parameters["@pprocessname"].Value = item.ProcessName;
                                insertSQL.Parameters["@ppid"].Value = item.PID;
                                insertSQL.Parameters["@ppath"].Value = item.Path;
                                insertSQL.Parameters["@parguments"].Value = item.Arguments;
                                insertSQL.Parameters["@pstarttime"].Value = item.StartTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                insertSQL.Parameters["@pkerneltime"].Value = item.KernalTime;
                                insertSQL.Parameters["@pusertime"].Value = item.UserTime;
                                insertSQL.Parameters["@pworkingset"].Value = item.WorkingSet;
                                insertSQL.Parameters["@pusername"].Value = item.UserName;
                                insertSQL.Parameters["@pishidden"].Value = item.IsHidden;
                                insertSQL.Parameters["@psid"].Value = item.SID;
                                insertSQL.Parameters["@psidtype"].Value = item.SIDType;
                                insertSQL.Parameters["@pparentname"].Value = item.ParentName;
                                insertSQL.Parameters["@pmd5"].Value = item.MD5;
                                insertSQL.Parameters["@pissigned"].Value = item.IsSigned;
                                insertSQL.Parameters["@pisverified"].Value = item.IsVerified;
                                insertSQL.Parameters["@psignaturestring"].Value = item.SignatureString;
                                insertSQL.Parameters["@pca"].Value = item.CA;
                                insertSQL.Parameters["@pcertsubject"].Value = item.CertSubject;

                                insertSQL.ExecuteNonQuery();
                                long identity = connection.LastInsertRowId;
                                InsertModulesDetails(item.ListProcessModule, identity, connection);
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void InsertModulesDetails(List<LoadedModule> modules, long processTableId, SQLiteConnection connection)
        {
            try
            {
                using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                {
                    insertSQL.CommandText = @"INSERT INTO modules("
                                               + "processtableid,"
                                               + "processid,"
                                               + "modulename,"
                                               + "modulepath,"
                                               + "md5,"
                                               + "issigned,"
                                               + "isverified,"
                                               + "signaturestring,"
                                               + "ca,"
                                               + "certsubject"
                                       + ") VALUES(@pprocesstableid,@pprocessid,@pmodulename,"
                                       + "@pmodulepath,@pmd5,@pissigned,@pisverified,@psignaturestring,@pca,@pcertsubject)";

                    insertSQL.Parameters.AddWithValue("@pprocesstableid", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pprocessid", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pmodulename", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pmodulepath", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pmd5", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pissigned", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pisverified", string.Empty);
                    insertSQL.Parameters.AddWithValue("@psignaturestring", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pca", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pcertsubject", string.Empty);

                    foreach (var item in modules)
                    {
                        insertSQL.Parameters["@pprocesstableid"].Value = processTableId;
                        insertSQL.Parameters["@pprocessid"].Value = item.ProcessId;
                        insertSQL.Parameters["@pmodulename"].Value = item.ModuleName;
                        insertSQL.Parameters["@pmodulepath"].Value = item.ModulePath;
                        insertSQL.Parameters["@pmd5"].Value = item.MD5;
                        insertSQL.Parameters["@pissigned"].Value = item.IsSigned;
                        insertSQL.Parameters["@pisverified"].Value = item.IsVerified;
                        insertSQL.Parameters["@psignaturestring"].Value = item.SignatureString;
                        insertSQL.Parameters["@pca"].Value = item.CA;
                        insertSQL.Parameters["@pcertsubject"].Value = item.CertSubject;

                        insertSQL.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void InsertNetworkconnectionDetails(List<winaudits.Networkconnection> networkconnection, int auditMasterId)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                        {
                            insertSQL.CommandText = @"INSERT INTO networkconnection(auditmasterid,"
                                                   + "processname,"
                                                   + "pid,"
                                                   + "path,"
                                                   + "state,"
                                                   + "created,"
                                                   + "localip,"
                                                   + "localport,"
                                                   + "remoteip,"
                                                   + "remoteport,"
                                                   + "protocol"
                                                             + ") VALUES(@pauditmasterid,@pprocessname,@ppid,@ppath,@pstate,@pcreated,@plocalip,@plocalport,@premoteip,@premoteport,@pprotocol)";

                            insertSQL.Parameters.AddWithValue("@pauditmasterid", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pprocessname", string.Empty);
                            insertSQL.Parameters.AddWithValue("@ppid", string.Empty);
                            insertSQL.Parameters.AddWithValue("@ppath", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pstate", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pcreated", string.Empty);
                            insertSQL.Parameters.AddWithValue("@plocalip", string.Empty);
                            insertSQL.Parameters.AddWithValue("@plocalport", string.Empty);
                            insertSQL.Parameters.AddWithValue("@premoteip", string.Empty);
                            insertSQL.Parameters.AddWithValue("@premoteport", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pprotocol", string.Empty);

                            foreach (var item in networkconnection)
                            {
                                insertSQL.Parameters["@pauditmasterid"].Value = auditMasterId;
                                insertSQL.Parameters["@pprocessname"].Value = item.ProcessName;
                                insertSQL.Parameters["@ppid"].Value = item.PID;
                                insertSQL.Parameters["@ppath"].Value = item.Path;
                                insertSQL.Parameters["@pstate"].Value = item.State;
                                insertSQL.Parameters["@pcreated"].Value = item.Created;
                                insertSQL.Parameters["@plocalip"].Value = item.LocalIP;
                                insertSQL.Parameters["@plocalport"].Value = item.LocalPort;
                                insertSQL.Parameters["@premoteip"].Value = item.RemoteIP;
                                insertSQL.Parameters["@premoteport"].Value = item.RemotePort;
                                insertSQL.Parameters["@pprotocol"].Value = item.Protocol;

                                insertSQL.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void InsertAutorunpointsDetails(List<Autorunpoints> autorunpoints, int auditMasterId)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {

                        using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                        {
                            insertSQL.CommandText = @"INSERT INTO autorunpoints(auditmasterid,"
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
                                                   + "certsubject"
                                                                        + ") VALUES(@pauditmasterid,@ptype,@pregistrypath,@pfilepath,@pisregistry,@pisfile,"
                                                                        + "@pregistrymodified,@pregistryowner,@pregistryvaluename,@pregistryvaluestring,@pfilecreated"
                                                                        + ",@pfilemodified,@pfileowner,@pfilemd5,@pissigned,@pisverified,@psignaturestring,@pca,@pcertsubject)";

                            insertSQL.Parameters.AddWithValue("@pauditmasterid", string.Empty);
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


                            foreach (var item in autorunpoints)
                            {
                                insertSQL.Parameters["@pauditmasterid"].Value = auditMasterId;
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

                                insertSQL.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }

                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void InsertPrefetchDetails(List<Prefetch> prefetch, int auditMasterId)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();

                    using (var transaction = connection.BeginTransaction())
                    {
                        using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                        {
                            insertSQL.CommandText = @"INSERT INTO prefetch(auditMasterId,"
                                                   + "filename,"
                                                   + "fullpath,"
                                                   + "lastrun,"
                                                   + "created,"
                                                   + "timesrun,"
                                                   + "size,"
                                                   + "hashofprefetch"
                                                + ") VALUES(@pauditMasterId,@pfilename,@pfullpath,@plastrun,@pcreated,@ptimesrun,@psize,@phashofprefetch)";

                            {
                                insertSQL.Parameters.AddWithValue("@pauditmasterid", string.Empty);
                                insertSQL.Parameters.AddWithValue("@pfilename", string.Empty);
                                insertSQL.Parameters.AddWithValue("@pfullpath", string.Empty);
                                insertSQL.Parameters.AddWithValue("@plastrun", string.Empty);
                                insertSQL.Parameters.AddWithValue("@pcreated", string.Empty);
                                insertSQL.Parameters.AddWithValue("@ptimesrun", string.Empty);
                                insertSQL.Parameters.AddWithValue("@psize", string.Empty);
                                insertSQL.Parameters.AddWithValue("@phashofprefetch", string.Empty);
                                foreach (var item in prefetch)
                                {
                                    insertSQL.Parameters["@pauditmasterid"].Value = auditMasterId;
                                    insertSQL.Parameters["@pfilename"].Value = item.FileName;
                                    insertSQL.Parameters["@pfullpath"].Value = item.FullPath;
                                    insertSQL.Parameters["@plastrun"].Value = item.LastRun;
                                    insertSQL.Parameters["@pcreated"].Value = item.Created;
                                    insertSQL.Parameters["@ptimesrun"].Value = item.TimesRun;
                                    insertSQL.Parameters["@psize"].Value = item.Size;
                                    insertSQL.Parameters["@phashofprefetch"].Value = item.HashOfPrefetch;

                                    insertSQL.ExecuteNonQuery();
                                    int prefetchID = (int)connection.LastInsertRowId;
                                    InsertPretchPaths(item.PrefetchPath, prefetchID, connection);
                                }
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void InsertPretchPaths(List<string> prefetchPaths, int prefetchID, SQLiteConnection connection)
        {
            try
            {
                using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                {
                    insertSQL.CommandText = @"INSERT INTO prefetchpaths(prefetchid, prefetchpath) VALUES(@pprefetchid, @pprefetchpath)";

                    insertSQL.Parameters.AddWithValue("@pprefetchid", string.Empty);
                    insertSQL.Parameters.AddWithValue("@pprefetchpath", string.Empty);
                    foreach (var item in prefetchPaths)
                    {
                        insertSQL.Parameters["@pprefetchid"].Value = prefetchID;
                        insertSQL.Parameters["@pprefetchpath"].Value = item;
                        insertSQL.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void InsertServicesDetails(List<Services> services, int auditMasterID)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                        {
                            insertSQL.CommandText = @"INSERT INTO services(auditmasterid,"
                                                       + "servicename,"
                                                       + "fullname,"
                                                       + "description,"
                                                       + "path,"
                                                       + "arguments,"
                                                       + "status,"
                                                       + "startmode,"
                                                       + "type,"
                                                       + "servicedll,"
                                                       + "startedas,"
                                                       + "pid,"
                                                       + "md5ofexecutable,"
                                                       + "md5servicedll,"
                                                       + "executablesigned,"
                                                       + "executablesignverified,"
                                                       + "executablecertissuer,"
                                                       + "executablecertsubject,"
                                                       + "dllsigned,"
                                                       + "dllsignedverified,"
                                                       + "dllcertissuer,"
                                                       + "dllcertsubject"
                                                             + ") VALUES(@pauditmasterid,@pservicename,@pfullname,@pdescription,"
                                                             + "@ppath,@parguments,@pstatus,@pstartmode,@ptype,@pservicedll,@pstartedas,@ppid,"
                                                             + "@pmd5ofexecutable,@pmd5servicedll,@pexecutablesigned,@pexecutablesignverified,"
                                                             + "@pexecutablecertissuer,@pexecutablecertsubject,@pdllsigned,@pdllsignedverified,"
                                                             + "@pdllcertissuer,@pdllcertsubject)";


                            insertSQL.Parameters.AddWithValue("@pauditmasterid", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pservicename", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pfullname", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pdescription", string.Empty);
                            insertSQL.Parameters.AddWithValue("@ppath", string.Empty);
                            insertSQL.Parameters.AddWithValue("@parguments", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pstatus", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pstartmode", string.Empty);
                            insertSQL.Parameters.AddWithValue("@ptype", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pservicedll", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pstartedas", string.Empty);
                            insertSQL.Parameters.AddWithValue("@ppid", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pmd5ofexecutable", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pmd5servicedll", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pexecutablesigned", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pexecutablesignverified", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pexecutablecertissuer", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pexecutablecertsubject", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pdllsigned", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pdllsignedverified", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pdllcertissuer", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pdllcertsubject", string.Empty);

                            foreach (var item in services)
                            {
                                insertSQL.Parameters["@pauditmasterid"].Value = auditMasterID;
                                insertSQL.Parameters["@pservicename"].Value = item.ServiceName;
                                insertSQL.Parameters["@pfullname"].Value = item.FullName;
                                insertSQL.Parameters["@pdescription"].Value = item.Description;
                                insertSQL.Parameters["@ppath"].Value = item.Path;
                                insertSQL.Parameters["@parguments"].Value = item.Arguments;
                                insertSQL.Parameters["@pstatus"].Value = item.Status;
                                insertSQL.Parameters["@pstartmode"].Value = item.StartMode;
                                insertSQL.Parameters["@ptype"].Value = item.Type;
                                insertSQL.Parameters["@pservicedll"].Value = item.ServiceDLL;
                                insertSQL.Parameters["@pstartedas"].Value = item.StartedAs;
                                insertSQL.Parameters["@ppid"].Value = item.PID;
                                insertSQL.Parameters["@pmd5ofexecutable"].Value = item.MD5OfExecutable;
                                insertSQL.Parameters["@pmd5servicedll"].Value = item.MD5ServiceDLL;
                                insertSQL.Parameters["@pexecutablesigned"].Value = item.ExecutableSigned;
                                insertSQL.Parameters["@pexecutablesignverified"].Value = item.ExecutableSignVerified;
                                insertSQL.Parameters["@pexecutablecertissuer"].Value = item.ExecutableCertIssuer;
                                insertSQL.Parameters["@pexecutablecertsubject"].Value = item.ExecutableCertSubject;
                                insertSQL.Parameters["@pdllsigned"].Value = item.DLLSigned;
                                insertSQL.Parameters["@pdllsignedverified"].Value = item.DLLSignedVerified;
                                insertSQL.Parameters["@pdllcertissuer"].Value = item.DLLCertIssuer;
                                insertSQL.Parameters["@pdllcertsubject"].Value = item.DLLCertSubject;

                                insertSQL.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void InsertDnsDetails(List<DNS> dns)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    foreach (var item in dns)
                    {
                        using (SQLiteCommand insertSQL = new SQLiteCommand(
                                                   @"INSERT INTO dns(dbid,"
                                                   + "host,"
                                                   + "recordname,"
                                                   + "ttl,"
                                                   + "datalength,"
                                                   + "flags,"
                                                   + "recordtype"
                                                   + ") VALUES(?,?,?,?,?,?,?)",
                                                   connection))
                        {
                            SQLiteParameter param1 = new SQLiteParameter();
                            insertSQL.Parameters.Add(param1);

                            param1 = new SQLiteParameter();
                            param1.Value = item.Host;
                            insertSQL.Parameters.Add(param1);

                            param1 = new SQLiteParameter();
                            param1.Value = item.RecordName;
                            insertSQL.Parameters.Add(param1);

                            param1 = new SQLiteParameter();
                            param1.Value = item.TTL;
                            insertSQL.Parameters.Add(param1);

                            param1 = new SQLiteParameter();
                            param1.Value = item.DataLength;
                            insertSQL.Parameters.Add(param1);

                            param1 = new SQLiteParameter();
                            param1.Value = item.Flags;
                            insertSQL.Parameters.Add(param1);

                            param1 = new SQLiteParameter();
                            param1.Value = item.RecordType;
                            insertSQL.Parameters.Add(param1);

                            insertSQL.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void InsertTaskDetails(List<RunningTasks> task, int auditMasterID)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                        {
                            insertSQL.CommandText = @"INSERT INTO task(auditmasterid,"
                                          + "taskname,"
                                          + "nextruntime,"
                                          + "status"
                                          + ") VALUES(@pauditmasterid,@ptaskname,@pnextruntime,@pstatus)";

                            insertSQL.Parameters.AddWithValue("@pauditmasterid", string.Empty);
                            insertSQL.Parameters.AddWithValue("@ptaskname", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pnextruntime", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pstatus", string.Empty);
                            foreach (var item in task)
                            {
                                insertSQL.Parameters["@pauditmasterid"].Value = auditMasterID;
                                insertSQL.Parameters["@ptaskname"].Value = item.TaskName;
                                insertSQL.Parameters["@pnextruntime"].Value = item.NextRunTime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss.fff");
                                insertSQL.Parameters["@pstatus"].Value = item.Status;
                                insertSQL.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void InsertArpDetails(List<ARP> arp, int auditMasterID)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                        {
                            insertSQL.CommandText = @"INSERT INTO arp(auditmasterid,"
                                          + "interface,"
                                          + "physicaladdress,"
                                          + "ip4address, cachetype"
                                          + ") VALUES(@pauditmasterid,@pinterface,@pphysicaladdress,@pip4address,@pcachetype)";

                            insertSQL.Parameters.AddWithValue("@pauditmasterid", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pinterface", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pphysicaladdress", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pip4address", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pcachetype", string.Empty);
                            foreach (var item in arp)
                            {
                                insertSQL.Parameters["@pauditmasterid"].Value = auditMasterID;
                                insertSQL.Parameters["@pinterface"].Value = item.Interface;
                                insertSQL.Parameters["@pphysicaladdress"].Value = item.PhysicalAddress;
                                insertSQL.Parameters["@pip4address"].Value = item.IP4Address;
                                insertSQL.Parameters["@pcachetype"].Value = item.CacheType;
                                insertSQL.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void InsertRecentlyInstall(List<InstalledApp> reginstall, int auditmasterid)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                        {
                            insertSQL.CommandText = @"INSERT INTO installedapp(auditmasterid,"
                                                             + "displayname,"
                                                             + "version,"
                                                             + "installdate,"
                                                             + "key,"
                                                             + "is64"
                                                             + ") VALUES(@pauditmasterid,@pdisplayname,@pversion,@pinstalldate,@pkey,@pis64)";

                            insertSQL.Parameters.AddWithValue("@pauditmasterid", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pdisplayname", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pversion", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pinstalldate", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pkey", string.Empty);
                            insertSQL.Parameters.AddWithValue("@pis64", string.Empty);

                            foreach (var item in reginstall)
                            {
                                insertSQL.Parameters["@pauditmasterid"].Value = auditmasterid;
                                insertSQL.Parameters["@pdisplayname"].Value = item.DisplayName;
                                insertSQL.Parameters["@pversion"].Value = item.Version;
                                insertSQL.Parameters["@pinstalldate"].Value = item.InstallDate;
                                insertSQL.Parameters["@pkey"].Value = item.Key;
                                insertSQL.Parameters["@pis64"].Value = item.Is64;

                                insertSQL.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                    }
                }
            }

            catch (Exception)
            {
                throw;
            }
        }

        public static void InsertFileFetch(winaudits.FileFetch FileFetch)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                        {
                            insertSQL.CommandText = @"INSERT INTO filefetchaudit(auditjobid,"
                                          + "filepath, receivedtime"
                                          + ") VALUES(@pauditjobid,@pfilepath, @preceivedtime)";

                            insertSQL.Parameters.AddWithValue("@pauditjobid", FileFetch.AuditJobID);
                            insertSQL.Parameters.AddWithValue("@pfilepath", FileFetch.FilePath);
                            insertSQL.Parameters.AddWithValue("@preceivedtime", DateTime.Now);

                            insertSQL.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        public static void InsertRegFetch(winaudits.RegistryFetch regFetch)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (var transaction = connection.BeginTransaction())
                    {
                        using (SQLiteCommand insertSQL = new SQLiteCommand(connection))
                        {
                            insertSQL.CommandText = @"INSERT INTO registryfetchaudit(auditjobid,"
                                          + "registrypath, registryhive, receivedtime"
                                          + ") VALUES(@pauditjobid,@pregistrypath,@pregistryhive, @preceivedtime)";

                            insertSQL.Parameters.AddWithValue("@pauditjobid", regFetch.AuditJobID);
                            insertSQL.Parameters.AddWithValue("@pregistrypath", regFetch.RegistryPath);
                            insertSQL.Parameters.AddWithValue("@pregistryhive", regFetch.RegistryHive);
                            insertSQL.Parameters.AddWithValue("@preceivedtime", DateTime.Now);

                            insertSQL.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
