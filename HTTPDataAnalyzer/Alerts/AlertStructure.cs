using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTTPDataAnalyzer
{
    public class AlertStructure
    {
        public class Alert
        {
           // public string Name { get; set; }
            public string UserName { get; set; }
            public Header Headers { get; set; }
            public _Detection Detection { get; set; }
            public Web_Request WebRequest { get; set; }
            public _File File { get; set; }
            public _Registry Registry { get; set; }
        }

        public class Header
        {
            public string Message { get; set; }
            public DateTime Time { get; set; }
        }

        public class _Detection
        {
            public string Source { get; set; }
            public string Severity { get; set; }
            public _Analysis Analysis { get; set; }
        }

        public class Web_Request
        {
            public DateTime Time { get; set; }
            public string TargetCountry { get; set; }
            public _Analysis Analysis { get; set; }
            public _Activity Activity { get; set; }
            public _Process Process { get; set; }
        }

        public class _Activity
        {
            public string Url { get; set; }
            public string Type { get; set; }
            public string FileType { get; set; }
            public string MD5 { get; set; }
        }

        public class _File
        {
            public string Path { get; set; }
            public string MD5 { get; set; }
            public string FileType { get; set; }
            public _Analysis Analysis { get; set; }
        }
        public class _Process
        {
            public string Path { get; set; }
            public string Type { get; set; }
            public int Pid { get; set; }
        }

        public class _Registry
        {
            public string key { get; set; }
            public string Value { get; set; }
            public _Analysis Analysis { get; set; }
        }

        public class _Analysis
        {

        }
    }
}
