using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace winaudits
{
    public class Autorunpoints
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("registrypath")]
        public string RegistryPath { get; set; }
        [JsonProperty("filepath")]
        public string FilePath { get; set; }
        [JsonProperty("isregistry")]
        public bool IsRegistry { get; set; }
        [JsonProperty("isfile")]
        public bool IsFile { get; set; }
        [JsonProperty("registrymodified")]
        public string RegistryModified { get; set; }
        [JsonProperty("registryowner")]
        public string RegistryOwner { get; set; }
        [JsonProperty("registryvaluename")]
        public string RegistryValueName { get; set; }
        [JsonProperty("registryvaluestring")]
        public string RegistryValueString { get; set; }
        [JsonProperty("filecreated")]
        public string FileCreated { get; set; }
        [JsonProperty("filemodified")]
        public string FileModified { get; set; }
        [JsonProperty("fileowner")]
        public string FileOwner { get; set; }
        [JsonProperty("filemd5")]
        public string FileMD5 { get; set; }
        [JsonProperty("issigned")]
        public bool IsSigned { get; set; }
        [JsonProperty("isverified")]
        public bool IsVerified { get; set; }
        [JsonProperty("signaturestring")]
        public string SignatureString { get; set; }
        [JsonProperty("ca")]
        public string CA { get; set; }
        [JsonProperty("certsubject")]
        public string CertSubject { get; set; }
    }

    internal class RegistryRun
    {
        public static List<Autorunpoints> StartAudit()
        {
            var lstAutoRuns = new List<Autorunpoints>();

            try
            {
                List<string> regprof = RegistryUtil.GetRegProfiles();

                foreach (var prf in regprof)
                {
                    AuditHive(prf, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false, "Run", lstAutoRuns);
                    AuditHive(prf, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce", false, "RunOnce", lstAutoRuns);
                    AuditHive(prf, "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Windows\\Run", false, "Run", lstAutoRuns);
                    AuditHive(prf, "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Windows\\Load", false, "Load", lstAutoRuns);
                }
                AuditHive("LocalMachine", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", false, "Run", lstAutoRuns);
                AuditHive("LocalMachine", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce", false, "RunOnce", lstAutoRuns);

                if (PlatformCheck.IsWow64() == true)
                {
                    AuditHive("LocalMachine", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true, "Run", lstAutoRuns);
                    AuditHive("LocalMachine", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce", true, "RunOnce", lstAutoRuns);
                }
            }
            catch (Exception)
            {

            }
            return lstAutoRuns;
        }

        private static void AuditHive(string hive, string run, bool is64, string runtype, List<Autorunpoints> regrunlist)
        {
            try
            {
                RegistryKey basekey = null;
                RegistryKey runkey = null;

                if (hive == "LocalMachine")
                {
                    basekey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, is64 == true ? RegistryView.Registry64 : RegistryView.Registry32);
                    runkey = basekey.OpenSubKey(run);

                }
                else
                {
                    RegistryKey runhive = Registry.Users.OpenSubKey(hive);
                    if (runhive != null)
                    {
                        runkey = runhive.OpenSubKey(run);
                    }
                }
                string owner = string.Empty;
                if (runkey != null)
                {
                    DateTime regModified = RegistryModified.lastWriteTime(runkey);
                    owner = RegistryUtil.GetRegKeyOwner(runkey);
                    foreach (var value in runkey.GetValueNames())
                    {
                        Autorunpoints autoruns = new Autorunpoints();
                        try
                        {
                            string keyValue = Convert.ToString(runkey.GetValue(value));

                            if (hive == "LocalMachine")
                            {
                                autoruns.RegistryPath = "LocalMachine\\" + run;
                            }
                            else
                            {
                                autoruns.RegistryPath = hive + "\\" + run;
                            }
                            autoruns.RegistryValueString = keyValue;
                            autoruns.RegistryValueName = value;

                            if (!string.IsNullOrEmpty(autoruns.RegistryValueString))
                            {
                                string[] pathAndArgument = autoruns.RegistryValueString.Split(new string[] { " -", " /", " \"" }, 2, StringSplitOptions.RemoveEmptyEntries);
                                if (pathAndArgument.Length > 0)
                                {

                                    autoruns.RegistryValueString = pathAndArgument[0].Replace("\"", string.Empty);
                                    autoruns.FilePath = autoruns.RegistryValueString;
                                }
                            }
                            autoruns.IsRegistry = true;
                            autoruns.RegistryOwner = owner;
                            autoruns.RegistryModified = regModified.ToString(DBManager.DateTimeFormat);
                            autoruns.Type = runtype;
                            regrunlist.Add(autoruns);
                        }
                        catch (Exception)
                        {

                        }
                    }
                    if (basekey != null)
                    {
                        basekey.Close();
                    }
                    runkey.Close();
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
