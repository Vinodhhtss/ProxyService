using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace winaudits
{
    public class UpdateQuery
    {
        public static void ExcecuteUpdateQuery(int paramValue, int jobid)
        {
            if (paramValue == 4)
            {
                RemoveOldAudits();
            }
            string query = string.Empty;
            if (paramValue == 4)
            {
                query = "UPDATE auditmaster SET status = @pparamValue, completetime = @pcompletetime WHERE dbid = " + jobid;
            }
            else
            {
                query = "UPDATE auditmaster SET status = @pparamValue WHERE dbid = " + jobid;
            }
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@pparamValue", paramValue);
                        if (paramValue == 4)
                        {
                            cmd.Parameters.AddWithValue("@pcompletetime", DateTime.Now);
                        }
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                //Logger.Error(ex);
            }
        }

        public static void RemoveOldAudits()
        {

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();

                    using (SQLiteCommand cmd = new SQLiteCommand("PRAGMA foreign_keys = ON", connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (SQLiteCommand cmd = new SQLiteCommand("DELETE FROM  auditmaster WHERE status = 4", connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                //Logger.Error(ex);
            }
        }


        public static void UpdateAuditInitiateStatus(SQLiteConnection connection, int jobid)
        {
            string query = "UPDATE auditmaster SET status = @pparamValue, initiatedTime = @pinitiatedTime  WHERE dbid = " + jobid;
            try
            {

                using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@pparamValue", 1);
                    cmd.Parameters.AddWithValue("@pinitiatedTime", DateTime.Now);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                //Logger.Error(ex);
            }
        }


        public static void UpdateFileFetchAuditStatus(int status, int id)
        {
            string query = string.Empty;

            if (status == 2)
            {
                query = "UPDATE filefetchaudit SET status = @pparamValue, completetime = @pcompletetime WHERE auditjobid = " + id;
            }
            else
            {
                query = "UPDATE filefetchaudit SET status = @pparamValue WHERE auditjobid = " + id;
            }
           
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@pparamValue", status);
                        if (status == 2)
                        {
                            cmd.Parameters.AddWithValue("@pcompletetime", DateTime.Now);
                        }
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                //Logger.Error(ex);
            }
        }


        public static void UpdateRegistryFetchAuditStatus(int status, int id)
        {
            string query = string.Empty;

            if (status == 2)
            {
                query = "UPDATE registryfetchaudit SET status = @pparamValue, completetime = @pcompletetime WHERE auditjobid = " + id;
            }
            else
            {
                query = "UPDATE registryfetchaudit SET status = @pparamValue WHERE auditjobid = " + id;
            }

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(DBManager.ConnectionString))
                {
                    connection.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@pparamValue", status);
                        if (status == 2)
                        {
                            cmd.Parameters.AddWithValue("@pcompletetime", DateTime.Now);
                        }
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                //Logger.Error(ex);
            }
        }
    }
}
