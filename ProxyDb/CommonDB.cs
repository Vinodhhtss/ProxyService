using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace ProxyDbs
{
    public class CommonDB
    {
        internal static string SQ_DB_LOCATION
        {
            get
            {
                return System.IO.Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData),
                        Path.Combine(RequiredTables.APP_DATA_FOLDER, RequiredTables.SQLITE_FOLDER));
            }
        }

        internal static string ConnectionString
        {
            get
            {
                return "Data Source=" + SQ_DB_LOCATION + ";Version=3;PRAGMA journal_mode=WAL;";
            }
        }

        public bool CheckAndCreateDb()
        {
            try
            {
                if (!System.IO.File.Exists(SQ_DB_LOCATION))
                {
                    SQLiteConnection.CreateFile(SQ_DB_LOCATION);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return true;
        }

        internal bool ResetAndCreateDb()
        {
            try
            {
                if (System.IO.File.Exists(SQ_DB_LOCATION))
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        File.Delete(SQ_DB_LOCATION);
                    }
                    else
                    {
                        return false;
                    }
                }
                string dirPath = Path.GetDirectoryName(SQ_DB_LOCATION);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                SQLiteConnection.CreateFile(SQ_DB_LOCATION);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }


        internal bool InitializeDatabase()
        {
            bool retval = true;
            int rval = 0;
            try
            {

                RequiredTables db = new RequiredTables();
                IList<string> lstProperties = new List<string>();
                foreach (var prop in db.GetType().GetFields())
                {
                    lstProperties.Add(prop.GetValue(db).ToString());
                }

                using (SQLiteConnection connection = new SQLiteConnection(CommonDB.ConnectionString))
                {
                    connection.Open();

                    foreach (var table in lstProperties)
                    {
                        using (SQLiteCommand command = new SQLiteCommand(table, connection))
                        {
                            rval = command.ExecuteNonQuery();
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }

            return retval;
        }
    }
}
