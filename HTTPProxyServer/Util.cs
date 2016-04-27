using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace HTTPProxyServer
{
    class Util
    {
        public static string DoGetHostEntry(string hostname)
        {
            string ipAddress = string.Empty;

            try
            {
                if (!IsIPv4(hostname))
                {
                    IPHostEntry host = Dns.GetHostEntry(hostname);
                    ipAddress = FindIpAdrress(host.AddressList);
                }
                else
                {
                    ipAddress = hostname;
                }
            }
            catch (Exception ex)
            {
                ////TCPClientProcessor.Proxylog.Logger.Error(ex);
            }

            return ipAddress;
        }


        public static bool IsIPv4(string value)
        {
            IPAddress address;

            if (IPAddress.TryParse(value, out address))
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    return true;
                }
            }

            return false;
        }

        public static string FindIpAdrress(IPAddress[] ipAddresses)
        {
            string myIP = string.Empty;
            try
            {
                foreach (var ipAddress in ipAddresses)
                {
                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ipAddress))
                    {
                        myIP = ipAddress.ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ////TCPClientProcessor.Proxylog.Logger.Error(ex);
            }

            return myIP;
        }

        public static bool ReadProxyConfig()
        {
            try
            {
                string pathOfConfigFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                pathOfConfigFile = Path.Combine(pathOfConfigFile, "ProxyConfig.xml");
                string text = File.ReadAllText(pathOfConfigFile);
                UpStreamProxy tempUpStream = DeSerializeToUpStream(text);
                if (IsIPv4(tempUpStream.IPAddress))
                {
                    if (tempUpStream.Port > 1 && tempUpStream.Port < 65535)
                    {
                        ProxyConfig.UpStream = tempUpStream;
                        ProxyConfig.AssignWebProxy();
                    }
                }

            }
            catch (Exception)
            {
                ProxyConfig.UpStream = null;
            }
            return true;
        }

        public static UpStreamProxy DeSerializeToUpStream(string xml)
        {
            XmlSerializer ser = new XmlSerializer(typeof(UpStreamProxy));
            StringReader stringReader;
            stringReader = new StringReader(xml);
            XmlTextReader xmlReader;
            xmlReader = new XmlTextReader(stringReader);
            UpStreamProxy configParams = (UpStreamProxy)ser.Deserialize(xmlReader);
            xmlReader.Close();
            stringReader.Close();

            return configParams;
        }
    }
}
