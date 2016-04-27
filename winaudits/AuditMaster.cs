using Newtonsoft.Json;
using System;

namespace winaudits
{
    public class AuditMaster
    {
        [JsonProperty("dbid")]
        public int ClientJobID { get; set; }
        [JsonProperty("auditjobidserver")]
        public int ServerJobID { get; set; }
        [JsonProperty("includeuser")]
        public int IncludeUser { get; set; }
        [JsonProperty("includeprocess")]
        public int IncludeProcess { get; set; }
        [JsonProperty("includenetworkinfo")]
        public int IncludeNetworkInfo { get; set; }
        [JsonProperty("includeautorunpoints")]
        public int IncludeAutoRunPoints { get; set; }
        [JsonProperty("includeprefetch")]
        public int IncludePrefetch { get; set; }
        [JsonProperty("includeservices")]
        public int IncludeServices { get; set; }
        [JsonProperty("includedns")]
        public int IncludeDns { get; set; }
        [JsonProperty("includearp")]
        public int IncludeArp { get; set; }
        [JsonProperty("includeinstalledapp")]
        public int IncludeInstalledApp { get; set; }
        [JsonProperty("includetask")]
        public int IncludeTask { get; set; }
        [JsonProperty("initiatedtime")]
        public DateTime? InitiatedTime { get; set; }
        [JsonProperty("receivedtime")]
        public DateTime? ReceivedTime { get; set; }
        [JsonProperty("status")]
        public int Status { get; set; }

        public static AuditMaster GetAuditMaster(string auditMas)
        {
            AuditMaster audit = JsonConvert.DeserializeObject<AuditMaster>(auditMas);
            return audit;
        }
    }

    public class FileFetch
    {
        [JsonProperty("auditjobid")]
        public int AuditJobID { get; set; }
        [JsonProperty("filepath")]
        public string FilePath { get; set; }

        public static FileFetch GetFileFetch(string filefetchaudit)
        {
            FileFetch filefetch = JsonConvert.DeserializeObject<FileFetch>(filefetchaudit);
            return filefetch;
        }
    }

    public class RegistryFetch
    {
        [JsonProperty("auditjobid")]
        public int AuditJobID { get; set; }
        [JsonProperty("registrypath")]
        public string RegistryPath { get; set; }
        [JsonProperty("registryhive")]
        public int RegistryHive { get; set; }

        public static RegistryFetch GetFileFetch(string regAudit)
        {
            RegistryFetch oregAudit = JsonConvert.DeserializeObject<RegistryFetch>(regAudit);
            return oregAudit;
        }
    }
}
