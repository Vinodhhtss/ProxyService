namespace ProxyDbs
{
    internal class RequiredTables
    {
        public static string APP_DATA_FOLDER
        {
            get { return "ProxyService"; }
        }

        public static string SQLITE_FOLDER
        {
            get { return "Data\\proxy.sqlite"; }
        } 

        public string REQUEST_TABLE = @"Create table request (dbid  INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                          start_time DATETIME,
                                                                          process_name varchar,
                                                                          process_id varchar,
                                                                          host_name varchar,
                                                                          first_header_line varchar,
                                                                          url varchar,
                                                                          method varchar,
                                                                          server_ip varchar,
                                                                          thread_index varchar,
                                                                          request_headers text,
                                                                          request_body text,
                                                                          request_size int,
                                                                          islimitexceeds bool default 0,
                                                                          upload_mime_dll varchar,
                                                                          upload_mime_singature varchar,
                                                                          request_end DATETIME)";

        public string REQUEST_UPLOAD_FILE_DETAILS = @"Create table upload_file_details(dbid INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                                              request_id INTEGER,
                                                                                              uploaded_md5 varchar,
                                                                                              signer varchar,
                                                                                              version varchar,
                                                                                              FOREIGN KEY(request_id) REFERENCES request(dbid)  
                                                                                                )";

        public string REPONSE_TABLE = @"Create table response (dbid INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                          request_id INTEGER,
                                                                          response_start DATETIME,
                                                                          status_code int,
                                                                          content_type varchar, 
                                                                          response_headers text,                                                                         
                                                                          response_body text,
                                                                          response_size int,
                                                                          islimitexceeds bool default 0,
                                                                          download_mime_dll varchar,
                                                                          download_mime_singature varchar,
                                                                          lazy_status bool default 0,                  
                                                                          response_end DATETIME,
                                                                          FOREIGN KEY(request_id) REFERENCES request(dbid)
                                                                          )";

        //0- Default ;1- processed; 2 - marked ;3 - sent  

        public string REQUEST_DOWNLOAD_FILE_DETAILS = @"Create table download_file_details(dbid INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                                              request_id INTEGER,
                                                                                              downloaded_md5 varchar,
                                                                                              signer varchar,
                                                                                              version varchar,
                                                                                              isPartial bool default 0,
                                                                                              FOREIGN KEY(request_id) REFERENCES response(dbid)  
                                                                                                )";

        public string ALERTS = @"Create table alerts(dbid INTEGER PRIMARY KEY AUTOINCREMENT, message BLOB)";

        public string ALERT_FAILED_TABLE = @"Create table alert_failed (dbid INTEGER PRIMARY KEY AUTOINCREMENT, message BLOB)";

        public string LAZY_FAILED_TABLE = @"Create table lazy_failed (dbid INTEGER PRIMARY KEY AUTOINCREMENT, message BLOB)";

        public string DNS_TABLE = @"Create table dnsdata (dbid  INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                          start_time DATETIME,
                                                                          process_name varchar,
                                                                          process_id varchar,
                                                                          dns_name varchar,
                                                                          file_name varchar
                                                                          )";
        public string Process = @"Create table process (dbid  INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                          start_time DATETIME,
                                                                          process_id varchar,
                                                                          path varchar,
                                                                          commandline varchar,
                                                                          is64bit bool default 0,
                                                                          parentid varchar,
                                                                          end_time DATETIME
                                                                          )";
        public string Registry = @"Create table registry  (dbid  INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                          time DATETIME,
                                                                          process_id varchar,
                                                                          key varchar,
                                                                          value varchar,
                                                                          daatype varchar,
                                                                          data varchar,
                                                                          is64success bool default 0
                                                                          )";
        public string File_Creation = @"Create table file_creation (dbid  INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                          time DATETIME,
                                                                          file_path varchar,
                                                                          file_type uint16,
                                                                          issuccess bool default 0,
                                                                          md5 varchar,
                                                                          signature varchar,
                                                                          version varchar
                                                                          )";

        public string Detection  = @"Create table detection  (dbid  INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                          time DATETIME,
                                                                          message varchar,
                                                                          process_id varchar,
                                                                          detection_source varchar
                                                                          )";

    }
}
