using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace winaudits
{
    public static class RegistryUtil
    {
        public static string GetStringSubValue(string hive, string sub, string valuename, bool is64)
        {
            string valdata = null;
            RegistryKey basekey = null;
            RegistryKey subkey = null;
            try
            {

                if (hive == "LocalMachine")
                {
                    basekey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, is64 == true ? RegistryView.Registry64 : RegistryView.Registry32);
                    subkey = basekey.OpenSubKey(sub);
                }
                else
                {
                    RegistryKey runhive = Registry.Users.OpenSubKey(hive);
                    if (runhive != null)
                    {
                        subkey = runhive.OpenSubKey(sub);
                    }
                }

                if (subkey != null)
                {
                    string val = Convert.ToString(subkey.GetValue(valuename));

                    if (val != null)
                    {
                        return val;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (basekey != null)
                {
                    basekey.Close();
                }

                if (subkey != null)
                {
                    subkey.Close();
                }
            }

            return valdata;
        }

        public static string[] GetSubKeys(string hive, string sub, bool is64)
        {
            string[] localstring = null;

            RegistryKey basekey = null;
            RegistryKey subkey = null;
            try
            {
                if (hive == "LocalMachine")
                {
                    basekey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, is64 == true ? RegistryView.Registry64 : RegistryView.Registry32);
                    subkey = basekey.OpenSubKey(sub);
                }
                else
                {
                    RegistryKey runhive = Registry.Users.OpenSubKey(hive);
                    if (runhive != null)
                    {
                        subkey = runhive.OpenSubKey(sub);
                    }
                }

                if (subkey != null)
                {
                    localstring = subkey.GetSubKeyNames();
                }
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (basekey != null)
                {
                    basekey.Close();
                }

                if (subkey != null)
                {
                    subkey.Close();
                }
            }
            return localstring;
        }

        public static string[] GetSubValueNames(string sub, bool is64)
        {
            string[] localstring = null;

            RegistryKey basekey = null;
            RegistryKey subkey = null;
            try
            {
                basekey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, is64 == true ? RegistryView.Registry64 : RegistryView.Registry32);
                subkey = basekey.OpenSubKey(sub);

                if (subkey != null)
                {
                    localstring = subkey.GetValueNames();
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (basekey != null)
                {
                    basekey.Close();
                }

                if (subkey != null)
                {
                    subkey.Close();
                }
            }
            return localstring;
        }


        public static string GetMachineRegKeyOwner(string runkey, bool is64, out string regModified)
        {
            regModified = string.Empty;
            RegistryKey basekey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, is64 == true ? RegistryView.Registry64 : RegistryView.Registry32);
            RegistryKey subkey = basekey.OpenSubKey(runkey);
            string owner = string.Empty;
            try
            {
                if (subkey != null)
                {
                    System.Security.AccessControl.RegistrySecurity regSec = subkey.GetAccessControl();
                    owner = regSec.GetOwner(typeof(NTAccount)).ToString(); 
                    try
                    {

                        DateTime regMod = RegistryModified.lastWriteTime(subkey);
                        regModified = regMod.ToUniversalTime().ToString(DBManager.DateTimeFormat);
                    }
                    catch (Exception)
                    {
                    }

                }
            }
            catch (Exception)
            {

            }
            finally
            {
                if (basekey != null)
                {
                    basekey.Close();
                }
                if (subkey != null)
                {
                    subkey.Close();
                }
            }
            return owner;
        }

        public static string GetRegKeyOwner(RegistryKey runkey)
        {
            string owner = string.Empty;
            try
            {
                System.Security.AccessControl.RegistrySecurity regSec = runkey.GetAccessControl();
                owner = regSec.GetOwner(typeof(NTAccount)).ToString();
            }
            catch (Exception)
            {
            }
            return owner;
        }

        public static List<string> GetRegProfiles()
        {
            List<string> profiles = new List<string>();
            try
            {
                RegistryKey profkey = null;
                profkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProfileList");
                if (profkey != null)
                {
                    foreach (var value in profkey.GetSubKeyNames())
                    {
                        string subkeylocal = value.ToUpper();
                        if (subkeylocal.TrimStart().StartsWith("S-1-5-21") == true)
                        {
                            profiles.Add(subkeylocal);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return profiles;
            }

            return profiles;
        }

        public static List<string> GetUserProfiles()
        {
            List<string> profiles = new List<string>();
            try
            {
                RegistryKey profkey = null;
                profkey = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.Users, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
                if (profkey != null)
                {
                    foreach (var value in profkey.GetSubKeyNames())
                    {
                        string subkeylocal = value.ToUpper();
                        if (subkeylocal.TrimStart().StartsWith("S-1-5-21") && !subkeylocal.ToLower().EndsWith("_classes"))
                        {
                            profiles.Add(subkeylocal);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return profiles;
            }

            return profiles;
        }
    }
}
