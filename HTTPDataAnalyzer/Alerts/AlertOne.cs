using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace HTTPDataAnalyzer
{
    class AlertOne : IAlert
    {
        public bool IsAlert(SessionHandler oSessionHandler)
        {
            return true;
        }

        public void SendAlert(SessionHandler oSessionHandler)
        {
            string json = CreateJson(oSessionHandler);
            StringBuilder jsonAndHAR = new StringBuilder(json);
            jsonAndHAR.Append("\n\n");

            byte[] xelemBytes = Encoding.ASCII.GetBytes(jsonAndHAR.ToString());

            Queue<SessionHandler> tempSessions = new Queue<SessionHandler>();
            tempSessions.Enqueue(oSessionHandler);
            byte[] output = JSONCircularBuffer.ExportToHAR(tempSessions);
            output = Util.CombineByteArray(xelemBytes, output);

            try
            {
                AnalyzerManager.ProxydbObj.InsertInAlerts(output);
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
            }

            if (TestTCPClient.TestConfig.TestCheck)
            {
                TestTCPClient.SendAlertMessageToServer("2", output);
            }
            else
            {
                TCPClients.SendAlertMessageToServer("2", output);
            }
        }

        public static byte[] BinarySerialize(Object obj)
        {
            byte[] serializedObject = null;
            MemoryStream ms = new MemoryStream();
            BinaryFormatter b = new BinaryFormatter();
            try
            {
                b.Serialize(ms, obj);
                ms.Seek(0, 0);
                serializedObject = ms.ToArray();
                ms.Close();
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
            }

            return serializedObject;
        }

        public static string CreateJson(SessionHandler oSessionHandler)
        {
            string json = string.Empty;

            AlertStructure.Alert alert = new AlertStructure.Alert();
            alert.UserName = oSessionHandler.RequestLines["WINDOWSUSERNAME"];
            alert.Headers = new AlertStructure.Header();
            alert.Headers.Message = "Suspicious - alert";
            alert.Headers.Time = DateTime.Now;
            alert.Detection = new AlertStructure._Detection();
            alert.Detection.Source = oSessionHandler.ClientName;
            alert.WebRequest = new AlertStructure.Web_Request();
            alert.WebRequest.Time = oSessionHandler.RequestStarted;
            alert.WebRequest.Activity = new AlertStructure._Activity();
            alert.WebRequest.Activity.Url = oSessionHandler.RequestURL;
            alert.WebRequest.Activity.Type = "Download";
            alert.WebRequest.Activity.FileType = oSessionHandler.DownloadStream.MIME_DLL;
            alert.WebRequest.Activity.MD5 = string.Empty;
            alert.WebRequest.Process = new AlertStructure._Process();
            alert.WebRequest.Process.Path = oSessionHandler.ClientName;
            alert.WebRequest.Process.Pid = oSessionHandler.ClientID;
            alert.WebRequest.Process.Type = oSessionHandler.ClientName;

            json = JsonConvert.SerializeObject(alert);
            return json;
        }

        public static string ComputeFileMD5(String filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    byte[] data = md5.ComputeHash(stream);
                    StringBuilder sBuilder = new StringBuilder();

                    // Loop through each byte of the hashed data  
                    // and format each one as a hexadecimal string. 
                    for (int i = 0; i < data.Length; i++)
                    {
                        sBuilder.Append(data[i].ToString("x2"));
                    }

                    // Return the hexadecimal string. 
                    return sBuilder.ToString();
                }
            }
        }

        public static PhysicalAddress GetMacAddress()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Only consider Ethernet network interfaces
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                    nic.OperationalStatus == OperationalStatus.Up)
                {
                    return nic.GetPhysicalAddress();
                }
            }
            return null;
        }
    }
}
