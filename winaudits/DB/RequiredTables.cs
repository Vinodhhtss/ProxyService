namespace winaudits
{
   internal class RequiredTables
    {
        public static string APP_DATA_FOLDER
        {
            get { return "ProxyService"; }
        }

        public static string SQLITE_FOLDER
        {
            get { return "Data\\winaudit.sqlite"; }
        }

        public string AuditMaster = @"Create table auditmaster (dbid INTEGER PRIMARY KEY AUTOINCREMENT, auditjobidserver int, 
                                                                    includeuser int default 1, 
                                                                    includeprocess int default 1,
                                                                    includenetworkinfo int default 1, 
                                                                    includeautorunpoints int default 1, 
                                                                    includeprefetch int default 1, 
                                                                    includeservices int default 1, 
                                                                    includedns int default 1, 
                                                                    includearp int default 1, 
                                                                    includeinstalledapp int default 1,
                                                                    includetask int default 1,
                                                                    status int default 0, 
                                                                    receivedtime datetime, 
                                                                    initiatedtime datetime, 
                                                                    completetime datetime)";
        //Status  0-received, 1-initiated, 2-complete, 3-Error, 4-sent"

        public string USER = @"Create table user (dbid INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                    auditmasterid int, username string, 
                                                                    sid string, 
                                                                    fullname string,
                                                                    description string, 
                                                                    sidtype uint8, 
                                                                    lastlogin datetime, 
                                                                    disabled bool, 
                                                                    locked bool, 
                                                                    passwordrequired bool, 
                                                                    passwordage string, 
                                                                    groups string,
                                                                    FOREIGN KEY(auditmasterid) REFERENCES auditmaster(dbid)  ON DELETE CASCADE)";

        public string PROCESS = @"Create table process (dbid INTEGER PRIMARY KEY AUTOINCREMENT, auditmasterid int, processname string, 
                                                                    pid integer, 
                                                                    path string, 
                                                                    arguments string, 
                                                                    starttime datetime, 
                                                                    kerneltime uint64, 
                                                                    usertime uint64, 
                                                                    workingset integer,     
                                                                    username string, 
                                                                    ishidden bool, 
                                                                    sid string, 
                                                                    sidtype string, 
                                                                    parentname string, 
                                                                    ppid integer, 
                                                                    md5 string, 
                                                                    issigned bool, 
                                                                    isverified bool, 
                                                                    signaturestring string, 
                                                                    ca string, 
                                                                    certsubject string, 
                                                                    FOREIGN KEY(auditmasterid) REFERENCES auditmaster(dbid)  ON DELETE CASCADE)";


        public string MODULES = @"Create table modules (dbid INTEGER PRIMARY KEY AUTOINCREMENT, processtableid int, processid string, 
                                                                        modulename string, 
                                                                        modulepath string, 
                                                                        md5 string, 
                                                                        issigned bool, 
                                                                        isverified bool, 
                                                                        signaturestring string, 
                                                                        ca string,  
                                                                        certsubject string,
                                                                        FOREIGN KEY(processtableid) REFERENCES process(dbid)  ON DELETE CASCADE)";


        public string NETWORKCONNECTION = @"Create table networkconnection (dbid INTEGER PRIMARY KEY AUTOINCREMENT, auditmasterid int, processname string, 
                                                                            pid string, 
                                                                            path string, 
                                                                            state string, 
                                                                            created string, 
                                                                            localip string, 
                                                                            localport string, 
                                                                            remoteip string, 
                                                                            remoteport string, 
                                                                            protocol string,
                                                                            FOREIGN KEY(auditmasterid) REFERENCES auditmaster(dbid)  ON DELETE CASCADE)";


        public string AUTORUNPOINTS = @"Create table autorunpoints (dbid INTEGER PRIMARY KEY AUTOINCREMENT, auditmasterid int, type string, 
                                                                                registrypath string, 
                                                                                filepath string, 
                                                                                isregistry bool, 
                                                                                isfile bool, 
                                                                                registrymodified string, 
                                                                                registryowner string, 
                                                                                registryvaluename string, 
                                                                                registryvaluestring string, 
                                                                                filecreated string, 
                                                                                filemodified string, 
                                                                                fileowner string, 
                                                                                filemd5 string, 
                                                                                issigned bool, 
                                                                                isverified bool, 
                                                                                signaturestring string, 
                                                                                ca string, 
                                                                                certsubject string, 
                                                                                FOREIGN KEY(auditmasterid) REFERENCES auditmaster(dbid)  ON DELETE CASCADE)";

        public string PREFETCH = @"Create table prefetch (dbid INTEGER PRIMARY KEY AUTOINCREMENT, auditmasterid int, filename string, 
                                                                        fullpath string, 
                                                                        prefetchpath string, 
                                                                        lastrun string, 
                                                                        created string, 
                                                                        timesrun integer, 
                                                                        size integer, 
                                                                        hashofprefetch string,
                                                                        FOREIGN KEY(auditmasterid) REFERENCES auditmaster(dbid)  ON DELETE CASCADE)";


        public string PREFETCHPATHS = @"Create table prefetchpaths (dbid INTEGER PRIMARY KEY AUTOINCREMENT, prefetchid int,
                                                                        prefetchpath string, 
                                                                        FOREIGN KEY(prefetchid) REFERENCES prefetch(dbid)  ON DELETE CASCADE)";

        public string SERVICES = @"Create table services (dbid  INTEGER PRIMARY KEY AUTOINCREMENT, auditmasterid int, 
                                                                        servicename string, 
                                                                        fullname string, 
                                                                        description string, 
                                                                        path string, 
                                                                        arguments string, 
                                                                        status string, 
                                                                        startmode string, 
                                                                        type string, 
                                                                        servicedll string, 
                                                                        startedas string, 
                                                                        pid integer,
                                                                        md5ofexecutable string, 
                                                                        md5servicedll string, 
                                                                        executablesigned bool, 
                                                                        executablesignverified bool, 
                                                                        executablecertissuer string, 
                                                                        executablecertsubject string, 
                                                                        dllsigned bool, 
                                                                        dllsignedverified bool, 
                                                                        dllcertissuer string, 
                                                                        dllcertsubject string,
                                                                        FOREIGN KEY(auditmasterid) REFERENCES auditmaster(dbid)  ON DELETE CASCADE)";

        public string DNS = @"Create table dns (dbid  INTEGER PRIMARY KEY AUTOINCREMENT, auditmasterid int,
                                                                        host string, 
                                                                        recordname string, 
                                                                        ttl string, 
                                                                        datalength string, 
                                                                        flags string, 
                                                                        recordtype string,
                                                                        FOREIGN KEY(auditmasterid) REFERENCES auditmaster(dbid)  ON DELETE CASCADE)";

        public string TASK = @"Create table task (dbid  INTEGER PRIMARY KEY AUTOINCREMENT, auditmasterid int,
                                                                        taskname string, 
                                                                        nextruntime datatime, 
                                                                        status string,
                                                                        FOREIGN KEY(auditmasterid) REFERENCES auditmaster(dbid)  ON DELETE CASCADE)";

        public string ARP = @"Create table arp (dbid  INTEGER PRIMARY KEY AUTOINCREMENT, auditmasterid int,
                                                                        interface string, 
                                                                        physicaladdress string, 
                                                                        ip4address string, 
                                                                        cachetype string,
                                                                        FOREIGN KEY(auditmasterid) REFERENCES auditmaster(dbid)  ON DELETE CASCADE)";

        public string INSTALLEDAPP = @"Create table installedapp (dbid  INTEGER PRIMARY KEY AUTOINCREMENT, auditmasterid int,
                                                                                    displayname string,
                                                                                    version string,
                                                                                    installdate string,
                                                                                    key string,
                                                                                    is64 bool,
                                                                                    FOREIGN KEY(auditmasterid) REFERENCES auditmaster(dbid)  ON DELETE CASCADE)";

        public string FILEFETCHAUDIT = @"Create Table filefetchaudit(dbId INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                                auditjobid  integer,
                                                                                filepath string,
                                                                                status int default 0, 
                                                                                receivedtime datetime, 
                                                                                completetime datetime)";

        public string REGISTRYFETCHAUDIT = @"Create Table registryfetchaudit(dbId INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                                auditjobid  integer,
                                                                                registrypath string,
                                                                                registryhive integer,
                                                                                status int default 0, 
                                                                                receivedtime datetime, 
                                                                                completetime datetime)";
    }
}
