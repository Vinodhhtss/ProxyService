using System;
using System.Data;
using System.Data.SQLite;

namespace ProxyDbs
{
    public partial class ProxyDb
    {
        public DataTable GetTableFromDB(string selectSQL, string tableName)
        {
            DataTable dt = null;
            if(selectSQL == "InvalidID")
            {
                return dt;
            }           
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(CommonDB.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand(selectSQL, connection))
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

        public bool CheckRowExist(string selectSQL)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(CommonDB.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand selectCommand = new SQLiteCommand(selectSQL, connection))
                    {
                        var tempCount = selectCommand.ExecuteScalar();
                        if (tempCount != null)
                        {
                            int count = Convert.ToInt32(tempCount);

                            if (count > 0)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }

        public static void DeleteRowFromTable(string sqlCommand)
        {
            try
            {
               // lock (DBLock)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(CommonDB.ConnectionString))
                    {
                        connection.Open();
                        using (SQLiteCommand command = new SQLiteCommand(sqlCommand, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void InsertDnsDetails(DateTime time, string process, ushort pid, string dnsname, string filepath)
        {
            try
            {
              //  lock (DBLock)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(CommonDB.ConnectionString))
                    {
                        connection.Open();
                        using (SQLiteCommand insertSQL = new SQLiteCommand(
                                                    @"INSERT INTO dnsdata (dbid,"
                                                     + "start_time,"
                                                     + "process_name, process_id,"
                                                     + "dns_name, file_name"
                                                     + ") VALUES (?,?,?,?,?,?)",
                                                     connection))
                        {

                            SQLiteParameter param1 = new SQLiteParameter();
                            insertSQL.Parameters.Add(param1);

                            SQLiteParameter param2 = new SQLiteParameter();
                            param2.Value = time.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            insertSQL.Parameters.Add(param2);

                            SQLiteParameter param3 = new SQLiteParameter();
                            param3.Value = process;
                            insertSQL.Parameters.Add(param3);

                            SQLiteParameter param4 = new SQLiteParameter();
                            param4.Value = pid;
                            insertSQL.Parameters.Add(param4);

                            SQLiteParameter param5 = new SQLiteParameter();
                            param5.Value = dnsname;
                            insertSQL.Parameters.Add(param5);

                            SQLiteParameter param6 = new SQLiteParameter();
                            param6.Value = filepath;
                            insertSQL.Parameters.Add(param6);

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

        public void InsertFileCreationDetails(DateTime time, string filePath, UInt16 fileType, bool isSuccess, string md5, string signature, string version)
        {
            try
            {
               // lock (DBLock)
                {
                    using (SQLiteConnection connection = new SQLiteConnection(CommonDB.ConnectionString))
                    {
                        connection.Open();
                        using (SQLiteCommand insertSQL = new SQLiteCommand(
                                                    @"INSERT INTO file_creation (dbid,"
                                                     + "time,"
                                                     + "file_path, file_type,"
                                                     + "isSuccess, md5, signature, version"
                                                     + ") VALUES (?,?,?,?,?,?,?,?)",
                                                     connection))
                        {

                            SQLiteParameter param1 = new SQLiteParameter();
                            insertSQL.Parameters.Add(param1);

                            param1 = new SQLiteParameter();
                            param1.Value = time.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            insertSQL.Parameters.Add(param1);

                            param1 = new SQLiteParameter();
                            param1.Value = filePath;
                            insertSQL.Parameters.Add(param1);

                            param1 = new SQLiteParameter();
                            param1.Value = fileType;
                            insertSQL.Parameters.Add(param1);

                            param1 = new SQLiteParameter();
                            param1.Value = isSuccess;
                            insertSQL.Parameters.Add(param1);

                            param1 = new SQLiteParameter();
                            param1.Value = md5;
                            insertSQL.Parameters.Add(param1);

                            param1 = new SQLiteParameter();
                            param1.Value = signature;
                            insertSQL.Parameters.Add(param1);

                            param1 = new SQLiteParameter();
                            param1.Value = version;
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
    }
}
