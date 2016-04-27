using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace winaudits
{
    internal class DelayedLoad
    {
        public static List<Autorunpoints> StartAudit()
        {
            var lstAutoRuns = new List<Autorunpoints>();
            try
            {
                // DELAYLOAD
                string regModified;
                string[] regdl = RegistryUtil.GetSubValueNames("Software\\Microsoft\\Windows\\CurrentVersion\\ShellServiceObjectDelayLoad", false);
                string owner = RegistryUtil.GetMachineRegKeyOwner("Software\\Microsoft\\Windows\\CurrentVersion\\ShellServiceObjectDelayLoad", false, out regModified);
                GetCLSIDDetails(lstAutoRuns, regdl, owner, "ShellServiceObjectDelayLoad", regModified);

                // DELAYLOAD 64
                regdl = RegistryUtil.GetSubValueNames("Software\\Microsoft\\Windows\\CurrentVersion\\ShellServiceObjectDelayLoad", true);
                owner = RegistryUtil.GetMachineRegKeyOwner("Software\\Microsoft\\Windows\\CurrentVersion\\ShellServiceObjectDelayLoad", true, out regModified);
                GetCLSIDDetails(lstAutoRuns, regdl, owner, "ShellServiceObjectDelayLoad", regModified);
            }
            catch (Exception)
            {
            }

            return lstAutoRuns;
        }

        public static void GetCLSIDDetails(List<Autorunpoints> lstAutoRuns, string[] regdl, string owner, string type, string regModified)
        {
            if (regdl != null)
            {
                for (int i = 0; i < regdl.Length; i++)
                {
                    Autorunpoints runPoint = new Autorunpoints();


                    runPoint.RegistryPath = "LocalMachine\\Software\\Classes\\CLSID\\" + regdl[i];
                    runPoint.RegistryValueName = "Default";
                    runPoint.RegistryValueString = RegistryUtil.GetStringSubValue("LocalMachine",
                                             "Software\\Classes\\CLSID\\" + regdl[i], "", false);
                    runPoint.FilePath = RegistryUtil.GetStringSubValue("LocalMachine",
                                            "Software\\Classes\\CLSID\\" + regdl[i] + "\\InprocServer32", "", false);
                    runPoint.IsRegistry = true;
                    runPoint.RegistryOwner = owner;
                    runPoint.RegistryModified = regModified;
                    runPoint.Type = type;
                    lstAutoRuns.Add(runPoint);
                }
            }
        }


    }
}
