using System;
using System.Data.SQLite;

namespace ProxyDbs
{
    public partial class ProxyDb
    {
        private static Object DBLock = new Object();

        public const int SizeLimit = 1024 * 1024 * 5;
        public ProxyDb(bool createDB = false)
        {
            try
            {
                if (createDB)
                {
                    CommonDB dbc = new CommonDB();
                    if (dbc.ResetAndCreateDb())
                    {
                        dbc.InitializeDatabase();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void CreateDb()
        {
            try
            {
                CommonDB dbc = new CommonDB();
                if (dbc.ResetAndCreateDb())
                {
                    dbc.InitializeDatabase();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int InsertRequestDetails(DateTime stime, string process, ushort pid, string hostName, string url, string firstHeaderLine,
                        string method, string serverIP, int threadIndex, byte[] requestHeaders, byte[] requestBody, string uploadMIMEDLL, string uploadMIMESignature, DateTime etime)
        {
            int lastID = -1;
            lock (DBLock)
            {
                using (SQLiteConnection connection = new SQLiteConnection(CommonDB.ConnectionString))
                {
                    connection.Open();

                    try
                    {
                        //     using (var transaction = connection.BeginTransaction())
                        {
                            using (SQLiteCommand insertSQL = new SQLiteCommand(
                                                            @"INSERT INTO request (dbid,"
                                                             + "start_time,"
                                                             + "process_name, process_id,"
                                                             + "host_name, url, first_header_line , method, server_ip,"
                                                             + "thread_index,"
                                                             + "request_headers,"
                                                             + "request_body,"
                                                             + "request_size,"
                                                             + "islimitexceeds,"
                                                             + "upload_mime_dll,"
                                                             + "upload_mime_singature,"
                                                             + "request_end) VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?,?)",
                                                             connection))
                            {

                                SQLiteParameter param1 = new SQLiteParameter();
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                param1.Value = stime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                param1.Value = process;
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                param1.Value = pid;
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                param1.Value = hostName;
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                param1.Value = firstHeaderLine;
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                param1.Value = url;
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                param1.Value = method;
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                param1.Value = serverIP;
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                param1.Value = threadIndex;
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                if (requestHeaders != null)
                                {
                                    param1.Value = Convert.ToBase64String(requestHeaders);
                                }
                                else
                                {
                                    param1.Value = string.Empty;
                                }
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                SQLiteParameter sizeExceeds = new SQLiteParameter();
                                sizeExceeds.Value = false;
                                if (requestBody != null && requestBody.Length <= SizeLimit)
                                {
                                    param1.Value = Convert.ToBase64String(requestBody);
                                }
                                else
                                {
                                    param1.Value = string.Empty;
                                    if (requestBody != null)
                                    {
                                        sizeExceeds.Value = true;
                                    }
                                }
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                param1.Value = requestBody == null ? 0 : requestBody.Length;
                                insertSQL.Parameters.Add(param1);

                                insertSQL.Parameters.Add(sizeExceeds);

                                param1 = new SQLiteParameter();
                                param1.Value = uploadMIMEDLL;
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                param1.Value = uploadMIMESignature;
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                param1.Value = etime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                insertSQL.Parameters.Add(param1);

                                insertSQL.ExecuteNonQuery();

                            }

                            lastID = (int)connection.LastInsertRowId;
                            //transaction.Commit();
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

            return lastID;
        }

        public void InsertUploadDetails(int requestID, string md5, string signer, string version)
        {
            try
            {
                lock (DBLock)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(CommonDB.ConnectionString))
                    {
                        connection.Open();
                        //   using (var transaction = connection.BeginTransaction())
                        {
                            using (SQLiteCommand insertSQL = new SQLiteCommand(
                                                        @"INSERT INTO upload_file_details (dbid,"
                                                        + "request_id,"
                                                        + "uploaded_md5,"
                                                        + "signer,"
                                                        + "version"
                                                         + ") VALUES (?,?,?,?,?)",
                                                         connection))
                            {

                                SQLiteParameter param1 = new SQLiteParameter();
                                insertSQL.Parameters.Add(param1);

                                SQLiteParameter param2 = new SQLiteParameter();
                                param2.Value = requestID;
                                insertSQL.Parameters.Add(param2);

                                SQLiteParameter param3 = new SQLiteParameter();
                                param3.Value = md5;
                                insertSQL.Parameters.Add(param3);

                                SQLiteParameter param4 = new SQLiteParameter();
                                param4.Value = signer;
                                insertSQL.Parameters.Add(param4);

                                SQLiteParameter param5 = new SQLiteParameter();
                                param5.Value = version;
                                insertSQL.Parameters.Add(param5);

                                insertSQL.ExecuteNonQuery();
                            }
                            //  transaction.Commit();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertResponseDetails(DateTime stime, int requestID, int statusCode, string contentType,
                     byte[] responseHeaders, byte[] responseBody, string downloadMIMEDLL, string downloadMIMESignature, DateTime etime)
        {

            try
            {
                lock (DBLock)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(CommonDB.ConnectionString))
                    {
                        connection.Open();
                        //  using (var transaction = connection.BeginTransaction())
                        {
                            using (SQLiteCommand insertSQL = new SQLiteCommand(
                                                        @"INSERT INTO response (dbid, request_id,"
                                                         + "response_start,"
                                                         + "status_code, response_headers,"
                                                         + "content_type,"
                                                         + "response_body,"
                                                         + "response_size,"
                                                         + "islimitexceeds,"
                                                         + "download_mime_dll,"
                                                         + "download_mime_singature, response_end"
                                                         + ") VALUES (?,?,?,?,?,?,?,?,?,?,?,?)",
                                                         connection))
                            {

                                SQLiteParameter param1 = new SQLiteParameter();
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                param1.Value = requestID;
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                param1.Value = stime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                param1.Value = statusCode;
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                if (responseHeaders != null)
                                {
                                    param1.Value = Convert.ToBase64String(responseHeaders);
                                }
                                else
                                {
                                    param1.Value = string.Empty;
                                }
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                param1.Value = contentType;
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                SQLiteParameter sizeExceeds = new SQLiteParameter();
                                sizeExceeds.Value = false;
                                if (responseBody != null && responseBody.Length <= SizeLimit)
                                {
                                    param1.Value = Convert.ToBase64String(responseBody);

                                }
                                else
                                {
                                    param1.Value = string.Empty;
                                    if (responseBody != null)
                                        sizeExceeds.Value = true;
                                }
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                param1.Value = responseBody == null ? 0 : responseBody.Length;
                                insertSQL.Parameters.Add(param1);

                                insertSQL.Parameters.Add(sizeExceeds);

                                param1 = new SQLiteParameter();
                                param1.Value = downloadMIMEDLL;
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                param1.Value = downloadMIMESignature;
                                insertSQL.Parameters.Add(param1);

                                param1 = new SQLiteParameter();
                                param1.Value = etime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                insertSQL.Parameters.Add(param1);


                                insertSQL.ExecuteNonQuery();
                            }
                            //transaction.Commit();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertDownloadDetails(int requestID, string md5, string signer, string version, bool isPartial)
        {
            try
            {
                lock (DBLock)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(CommonDB.ConnectionString))
                    {
                        connection.Open();
                        //   using (var transaction = connection.BeginTransaction())
                        {
                            using (SQLiteCommand insertSQL = new SQLiteCommand(
                                                           @"INSERT INTO download_file_details (dbid,"
                                                           + "request_id,"
                                                           + "downloaded_md5,"
                                                           + "signer,"
                                                           + "version,"
                                                            + "isPartial) VALUES (?,?,?,?,?,?)",
                                                            connection))
                            {

                                SQLiteParameter param = new SQLiteParameter();
                                insertSQL.Parameters.Add(param);

                                param = new SQLiteParameter();
                                param.Value = requestID;
                                insertSQL.Parameters.Add(param);

                                param = new SQLiteParameter();
                                param.Value = md5;
                                insertSQL.Parameters.Add(param);

                                param = new SQLiteParameter();
                                param.Value = signer;
                                insertSQL.Parameters.Add(param);

                                param = new SQLiteParameter();
                                param.Value = version;
                                insertSQL.Parameters.Add(param);

                                param = new SQLiteParameter();
                                param.Value = isPartial;
                                insertSQL.Parameters.Add(param);

                                insertSQL.ExecuteNonQuery();
                            }
                            // transaction.Commit();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertInAlerts(byte[] message)
        {

            try
            {
                lock (DBLock)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(CommonDB.ConnectionString))
                    {
                        connection.Open();
                        // using (var transaction = connection.BeginTransaction())
                        {
                            using (SQLiteCommand insertSQL = new SQLiteCommand(@"INSERT INTO alerts (message"
                                                         + ") VALUES (?)", connection))
                            {

                                SQLiteParameter param = new SQLiteParameter();
                                param.Value = message;
                                insertSQL.Parameters.Add(param);

                                insertSQL.ExecuteNonQuery();
                            }
                            //  transaction.Commit();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertInAlertFailed(byte[] message)
        {

            try
            {
                lock (DBLock)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(CommonDB.ConnectionString))
                    {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction())
                        {
                            using (SQLiteCommand insertSQL = new SQLiteCommand(@"INSERT INTO alert_failed (message"
                                                         + ") VALUES (?)", connection))
                            {
                                SQLiteParameter param = new SQLiteParameter();
                                param.Value = message;
                                insertSQL.Parameters.Add(param);

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

        public void InsertInLazyFailed(byte[] message)
        {

            try
            {
                lock (DBLock)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(CommonDB.ConnectionString))
                    {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction())
                        {
                            using (SQLiteCommand insertSQL = new SQLiteCommand(@"INSERT INTO lazy_failed (message"
                                                         + ") VALUES (?)", connection))
                            {
                                SQLiteParameter param = new SQLiteParameter();
                                param.Value = message;
                                insertSQL.Parameters.Add(param);

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

        public void UpdateDB(string query)
        {
            try
            {
                lock (DBLock)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(CommonDB.ConnectionString))
                    {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction())
                        {
                            using (SQLiteCommand command = new SQLiteCommand(query, connection))
                            {
                                command.ExecuteNonQuery();
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
    }
}
