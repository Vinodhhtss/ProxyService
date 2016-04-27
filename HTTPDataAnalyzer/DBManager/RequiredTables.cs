using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTTPDataAnalyzer
{
    internal class RequiredTables
    {

        public static string APP_DATA_FOLDER
        {
            get { return "ProxyService"; }
        }

        public static string SQLITE_FOLDER
        {
            get { return "Data\\sysinfo.sqlite"; }
        }

        public string HOSTINFO = @"Create table hostinfo (dbid INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                hostname string,
                                                                agentid integer,
                                                                agentversion string,
                                                                timezone string,
                                                                bitlevel integer,
                                                                osedition string,
                                                                osservicepack string,
                                                                osname string,
                                                                oslastuptime datetime,
                                                                domainname string,
                                                                installdate datetime,
                                                                productid string,
                                                                processor string,
                                                                primaryuser string,
                                                                registereduser string,
                                                                acrobat string,
                                                                java string,
                                                                flash string, 
                                                                chasistype string )";

        public string NETWORKTYPE = @"Create table networktype (dbid INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                    hostinfoid integer,
                                                                    ipaddress string,
                                                                    type string,
                                                                    mac string )";

        public string BROWSER = @"Create table browser (dbid INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                    hostinfoid integer,
                                                                    name string,
                                                                    version string,
                                                                    proxyenabled int )";

        public string OFFICEAPPLICATION = @"Create table officeapplication (dbid INTEGER PRIMARY KEY AUTOINCREMENT,
                                                                                hostinfoid integer,
                                                                                name string,
                                                                                version string )";

        public string AUTORUNPOINTS = @"Create table autorunpoints (dbid INTEGER PRIMARY KEY AUTOINCREMENT,  hostinfoid integer,
                                                                                type string, 
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
                                                                                isdeleted bool,
                                                                                collecteddate string)";

        public string INSTALLEDAPP = @"Create table installedapp (dbid  INTEGER PRIMARY KEY AUTOINCREMENT, 
                                                                                        hostinfoid integer,
                                                                                        displayname string,
                                                                                        version string,
                                                                                        installdate string,
                                                                                        key string,
                                                                                        is64 bool,
                                                                                        isinstalled bool,
                                                                                        collecteddate string)";
    }
}
