using System;

namespace HTTPDataAnalyzer
{
    public class HostInfo
    {
        public string HostID { get; set; }
    }

    public class Agent
    {
        public int AgentID { get; set; }
        public string AgentVersion { get; set; }
    }

    public class ServerDetail
    {
        public string MainServerIP { get; set; }
        public int MainServerPort { get; set; }
        public string BackupServerIP { get; set; }
        public int BackupServerPort { get; set; }
    }

    public class Policy
    {
        public int PolicyId { get; set; }
        public string PolicyName { get; set; }
        public bool IsProxyEnabled { get; set; }
        public int HeartBeatInterval { get; set; }
        public int LazyUpload { get; set; }
        public int MaxRetention { get; set; }
        public int PolicyVersion { get; set; }
    }

    public class ByPass
    {
        public string ByPassString { get; set; }
    }

    public class UpStreamProxy
    {
        public string IPAddress { get; set; }
        public string Port { get; set; }
    }   

    public class ConfigParameters
    {
        public Agent AgentInstaller = new Agent();
        public ServerDetail ServerDetails = new ServerDetail();
        public Policy Policies = new Policy();
        public HostInfo HostInfoes = new HostInfo();
        public ByPass ByPassDetails = new ByPass();
        public UpStreamProxy UpStreamProxies = new UpStreamProxy();
    }
}

