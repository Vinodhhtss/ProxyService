using Microsoft.Win32;
using System;
using System.Text;

namespace AddRegisterEntriesInstaller
{
    class ReadProxySettings
    {
        public string ExistingIp = string.Empty;
        public static string AlertIp = "192.168.1.10:8872";
        public static void ReadSettings()
        {
            RegistryKey registry = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings\\Connections", true);
            //var defaultConn = registry.GetValue("DefaultConnectionSettings", null);
            //  if (defaultConn == RegistryValueKind.Binary)
            {
                StringBuilder sb = new StringBuilder();
                var value = (byte[])registry.GetValue("DefaultConnectionSettings", null);
                byte[] newValue = value;
                for (int i = 16; i < value.Length; i++)
                {
                    sb.Append((char)Convert.ToInt32(value[i]));
                }

                char[] ipValue = AlertIp.ToCharArray();
                for (int i = 0, j = 16; i < ipValue.Length; i++, j++)
                {
                    newValue[j] = (byte)((int)ipValue[i]);
                }

                registry.SetValue("DefaultConnectionSettings", newValue);
                registry.Close();
            }
        }

        public static string RegBinaryToString(byte[] arrValue)
        {
            StringBuilder strInfo = new StringBuilder();
            for (int i = 0; i < arrValue.Length; i++)
            {
                if (arrValue[i] != 0)
                {
                    strInfo.Append(Convert.ToChar(arrValue[i]));
                }
            }
            return strInfo.ToString();
        }

        public static string RegBinaryToString(string arrValue)
        {
            StringBuilder strInfo = new StringBuilder();
            for (int i = 0; i < arrValue.Length; i++)
            {
                if (arrValue[i] != 0)
                {
                    strInfo.Append(Convert.ToChar(arrValue[i]));
                }
            }
            return strInfo.ToString();
        }
    }

    class EditDefaultConnectionSettings
    {
        public static byte[] EditProxyServer(byte[] conString, string proxy)
        {
            char[] ipValue = proxy.ToCharArray();
            byte[] newValue = new byte[16 + ipValue.Length + 40];

            for (int i = 0; i < 16; i++)
            {
                newValue[i] = conString[i];
            }

            for (int i = 0, j = 16; i < ipValue.Length; i++, j++)
            {
                newValue[j] = (byte)((int)ipValue[i]);
            }

            for (int i = 16 + ipValue.Length; i < newValue.Length; i++)
            {
                newValue[i] = (byte)0;
            }

            return newValue;
        }

        public static byte[] EditProxyOverride(byte[] conString, string proxy)
        {
            if(proxy == null)
            {
                proxy = string.Empty;
            }
            char[] ipValue = proxy.ToCharArray();
            byte[] newValue = null;
            int lastPosition = 0;
            newValue = new byte[conString.Length + ipValue.Length + 40];

            if (ipValue.Length != 0)
            {
                newValue[conString.Length] = (byte)ipValue.Length;
            }

            lastPosition = conString.Length + 4;

            for (int i = 0, j = lastPosition; i < ipValue.Length; i++, j++)
            {
                newValue[j] = (byte)((int)ipValue[i]);
            }

            for (int i = 0; i < lastPosition - 4; i++)
            {
                newValue[i] = conString[i];
            }

            return newValue;
        }

        public static bool IsContainsProxyOverride(byte[] conString)
        {
            bool isContainsOverride = false;
            for (int i = 0, j = conString.Length - 1; i < 40; i++, j--)
            {
                if (conString[j] != 0)
                {
                    isContainsOverride = true;
                    break;
                }
            }
            return isContainsOverride;
        }
    }
}
