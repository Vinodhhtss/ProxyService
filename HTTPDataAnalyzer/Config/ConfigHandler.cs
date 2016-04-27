using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace HTTPDataAnalyzer
{
    public class ConfigHandler
    {
        public static ConfigParameters Config = new ConfigParameters();
        public static HostInfo HostInfoes = null;
        public static ServerDetail ServerDetail = null;
        public static Agent AgentDetail = null;

        public static string ReadConfigFile()
        {
            string xmlDoc = null;
            string pathToCopyConfigFile = Util.GetConfigFilePath();
            string decryptedText = string.Empty;
            try
            {
                if (ConstantVariables.IsInDebug)
                {
                    decryptedText = File.ReadAllText(pathToCopyConfigFile);
                }
                else
                {
                    byte[] encryptTextBytes = File.ReadAllBytes(pathToCopyConfigFile);
                    Encryption cf = new Encryption();
                    Byte[] decryptedTextBytes = cf.Decrypt(encryptTextBytes, null);
                    decryptedText = Encoding.UTF8.GetString(decryptedTextBytes);
                }
                xmlDoc = decryptedText;
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
            }
            return xmlDoc;
        }

        public static void EncryptAndSave(string plainText)
        {
            Encryption ef = new Encryption();
            byte[] originTextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedTextBytes = ef.Encrypt(originTextBytes, null);
            SaveConfigFile(encryptedTextBytes);
        }

        public static void NormalSaveConfigFile(string plainText)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XElement xelem = XElement.Parse(plainText);
            xelem.Save(Util.GetConfigFilePath());
        }

        public static void SaveConfigFile(string plainText)
        {
            if (!string.IsNullOrEmpty(plainText))
            {
                ConfigHandler.Config = DeSerializeToConfigParams(plainText);
            }

            if (ConfigHandler.ServerDetail == null)
            {
                ConfigHandler.ServerDetail = ConfigHandler.Config.ServerDetails;
            }

            if (ConfigHandler.AgentDetail == null)
            {
                ConfigHandler.AgentDetail = ConfigHandler.Config.AgentInstaller;
            }

            ConfigHandler.Config.HostInfoes = ConfigHandler.HostInfoes;
            ConfigHandler.Config.ServerDetails = ConfigHandler.ServerDetail;
            plainText = SerializeToXML(ConfigHandler.Config);
            if (ConstantVariables.IsInDebug)
            {
                NormalSaveConfigFile(plainText);

            }
            else
            {
                EncryptAndSave(plainText);
            }
        }

        private static void SaveConfigFile(byte[] encryptedTextBytes)
        {
            File.WriteAllBytes(Util.GetConfigFilePath(), encryptedTextBytes);
        }

        public static ConfigParameters DeSerializeToConfigParams(string xml)
        {
            XmlSerializer ser = new XmlSerializer(typeof(ConfigParameters));
            StringReader stringReader;
            stringReader = new StringReader(xml);
            XmlTextReader xmlReader;
            xmlReader = new XmlTextReader(stringReader);
            ConfigParameters configParams = (ConfigParameters)ser.Deserialize(xmlReader);
            xmlReader.Close();
            stringReader.Close();

            return configParams;
        }

        public static string SerializeToXML(ConfigParameters configParameter)
        {
            string toXML = string.Empty;
            XmlSerializer xsSubmit = new XmlSerializer(typeof(ConfigParameters));
            using (StringWriter sww = new StringWriter())
            using (XmlWriter writer = XmlWriter.Create(sww))
            {
                xsSubmit.Serialize(writer, configParameter);
                toXML = sww.ToString();

            }

            return toXML;
        }
    }
}
