using System;
using System.Collections.Generic;

namespace winaudits
{
    internal class RegistryBHO // BHO(Browser Helper Objects)
    {
        public static List<Autorunpoints> StartAudit()
        {
            var lstAutoRuns = new List<Autorunpoints>();
            try
            {
                ///// BHO
                string regModified;
                string[] regbhos = RegistryUtil.GetSubKeys("LocalMachine", "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Browser Helper Objects", false);
                string owner = RegistryUtil.GetMachineRegKeyOwner("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Browser Helper Objects", false, out regModified);
                DelayedLoad.GetCLSIDDetails(lstAutoRuns, regbhos, owner, "Browser Helper Objects", regModified);

                ///// BHO 64
                regbhos = RegistryUtil.GetSubKeys("LocalMachine", "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Browser Helper Objects", true);
                owner = RegistryUtil.GetMachineRegKeyOwner("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Browser Helper Objects", true, out regModified);
                DelayedLoad.GetCLSIDDetails(lstAutoRuns, regbhos, owner, "Browser Helper Objects", regModified);
            }
            catch (Exception)
            {
                return lstAutoRuns;
            }

            return lstAutoRuns;
        }
    }
}