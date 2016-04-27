using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace winaudits
{
    public class DNS
    {
        [JsonProperty("host")]
        public string Host { get; set; }
        [JsonProperty("recordname")]
        public string RecordName { get; set; }
        [JsonProperty("ttl")]
        public string TTL { get; set; }
        [JsonProperty("datalength")]
        public string DataLength { get; set; }
        [JsonProperty("flags")]
        public string Flags { get; set; }
        [JsonProperty("recordtype")]
        public string RecordType { get; set; }
    }   
}
