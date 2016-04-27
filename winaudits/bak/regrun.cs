using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace winaudits
{
    public class regrun
    {
        class regval {
            string hive;
            string key;
            string valuename;
            string value;
        };
        List<regval> regrunlist;

        List<string> keylist = { "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run" };

        public regrun()
        {
            regrunlist = new List<regval>();
        }

        public bool AuditStart()
        {

            
            return true;
        }

        public bool AuditHive(string hive, string run)
        {
            try
            {
                RegistryKey runkey = null;

                if (hive == "LocalMachine") {
                    runkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
                }
                else 
                {
                    RegistryKey runhive = Registry.Users.OpenSubKey(hive);
                    if (runhive != null) {
                        runkey = runhive.OpenSubKey(run);
                    }
                }
                if (runkey != null)
                {
                    foreach (var value in runkey.GetValueNames())
                    {
                        try
                        {
                            string keyValue = Convert.ToString(runkey.GetValue(value));
                            //regrunlist.Add(keyValue);
                            Console.WriteLine(value + " : " + keyValue);
                        }
                        catch (Exception ex)
                        {

                        }
                      
                    }
                }
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
        }



    }
}
