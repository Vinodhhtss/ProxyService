using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace HTTPDataAnalyzer
{
    class FileHandler
    {
        private static object Secure_Details = new object();
        public static void ReadFileName(SessionHandler oSessionHndlr, bool isUpload)
        {
            string tmpFilename = string.Empty;

            if (oSessionHndlr.FileName == null || oSessionHndlr.FileName == string.Empty)
            {
                try
                {
                    if (isUpload)
                    {
                        tmpFilename = oSessionHndlr.RequestLines["CONTENT-DISPOSITION"];
                    }
                    else
                    {
                        tmpFilename = oSessionHndlr.ResponseLines["CONTENT-DISPOSITION"];
                    }

                    if (tmpFilename.IndexOf('=') > 0)
                    {
                        tmpFilename = tmpFilename.Substring(tmpFilename.IndexOf('=') + 1);
                    }
                    else
                    {
                        tmpFilename = null;
                    }
                }
                catch (Exception ex)
                {
                    //AnalyzerManager.Logger.Error(ex);
                    tmpFilename = null;
                }
            }
            else
            {
                tmpFilename = oSessionHndlr.FileName;
            }

            if (tmpFilename != null && tmpFilename != string.Empty)
            {
                string tempTrimFileName = FileUtil.TrimFileName(tmpFilename);
                if (tempTrimFileName != string.Empty)
                {
                    oSessionHndlr.FileName = SaveFilePath(isUpload, tempTrimFileName);
                }
                else
                {
                    oSessionHndlr.FileName = null;
                }
                oSessionHndlr.FileName = FileUtil.GetSuggestedFileName(oSessionHndlr.FileName);
            }
            else
            {
                oSessionHndlr.FileName = null;
            }
        }

        public static void FileDetails(SessionHandler oSessionHndlr, bool isUpload)
        {
            string lines = "Using DLL Method" + "\t" + DateTime.Now.ToString("yyyyMMddHHmmss") + "\t" + Path.GetFileName(oSessionHndlr.FileName) + "\t" + oSessionHndlr.FileMIMETypeDLL;
            lines += Environment.NewLine + "Using Singature Method" + "\t" + DateTime.Now.ToString("yyyyMMddHHmmss") + "\t" + Path.GetFileName(oSessionHndlr.FileName) + "\t" + oSessionHndlr.FileMIMETypeSignature;
            lines += Environment.NewLine + Environment.NewLine;

            lock (Secure_Details)
            {
                using (StreamWriter w = File.AppendText(CreateDetailsFile(isUpload)))
                {
                    w.WriteLine(lines);
                }
            }
        }

        public static string CreateDetailsFile(bool isUpload)
        {
            string saveFilePath = string.Empty;
            if (isUpload)
            {
                if (!Directory.Exists(ConstantVariables.UPLOAD_FOLDER_PATH))
                {
                    Directory.CreateDirectory(ConstantVariables.UPLOAD_FOLDER_PATH);
                }
                saveFilePath = System.IO.Path.Combine(ConstantVariables.UPLOAD_FOLDER_PATH, "UploadDetails.txt");
            }
            else
            {
                if (!Directory.Exists(ConstantVariables.DOWLOAD_FOLDER_PATH))
                {
                    Directory.CreateDirectory(ConstantVariables.DOWLOAD_FOLDER_PATH);
                }
                saveFilePath = System.IO.Path.Combine(ConstantVariables.DOWLOAD_FOLDER_PATH, "DownloadDetails.txt");
            }
            return saveFilePath;
        }

        public static string SaveFilePath(bool isUpload, string fileName)
        {
            string saveFilePath = string.Empty;
            if (isUpload)
            {
                saveFilePath = ConstantVariables.UPLOAD_FOLDER_PATH;
                if (!Directory.Exists(saveFilePath))
                {
                    Directory.CreateDirectory(saveFilePath);
                }
                saveFilePath = System.IO.Path.Combine(saveFilePath, fileName);
            }
            else
            {
                saveFilePath = ConstantVariables.DOWLOAD_FOLDER_PATH;
                if (!Directory.Exists(saveFilePath))
                {
                    Directory.CreateDirectory(saveFilePath);
                }
                saveFilePath = System.IO.Path.Combine(saveFilePath, fileName);
            }

            return saveFilePath;
        }

        public static void GetFileName(SessionHandler oSessionHndlr, Byte[] buffer, bool isUpload)
        {
            FileHandler.ReadFileName(oSessionHndlr, isUpload);
        }

        public static void GetFileMIMEType(SessionHandler oSessionHndlr, Byte[] buffer, bool isUpload)
        {
            byte[] tempBuffer = buffer;
            if (oSessionHndlr.FileName != null)
            {
                oSessionHndlr.FileMIMETypeDLL = MIMEIdentifier.FileMIME.GetMimeFromBytes(tempBuffer);
                oSessionHndlr.FileMIMETypeSignature = MIMEIdentifier.FileMIMESignature.GetMimeType(tempBuffer);
                FileHandler.FileDetails(oSessionHndlr, isUpload);
            }
        }

        public static void WriteToFile(SessionHandler oSessionHndlr, Byte[] buffer, bool isUpload)
        {
            byte[] tempBuffer = buffer;
            if (!isUpload)
            {
                if (oSessionHndlr.ResponseLines.ContainsKey("CONTENT-ENCODING"))
                {
                    switch (oSessionHndlr.ResponseLines["CONTENT-ENCODING"])
                    {
                        case "gzip":
                            tempBuffer = Decompressor.DecompressGzip(new MemoryStream(tempBuffer), Encoding.Default);
                            break;
                    }
                }
            }

            if (oSessionHndlr.FileName != null)
            {
                try
                {
                    FileStream tempFilsStream = new FileStream(oSessionHndlr.FileName, FileMode.Create);
                    tempFilsStream.Write(tempBuffer, 0, tempBuffer.Length);
                    tempFilsStream.Close();
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message + " " + ex.StackTrace);
                }
            }

            if (oSessionHndlr.FileName == null || oSessionHndlr.FileName == string.Empty)
            {
                return;
            }

            string tempData = Convert.ToBase64String(tempBuffer);
           // if (LazyAnalyser.DataPacketsDB != null)
            {
                //if (isUpload)
                //{
                //    DBAnalyser.DataPacketsDB.InsertInUploads(oSessionHndlr.StartedDateTime.ToString(ConstantVariables.DATETIMEFORMAT),
                //                            oSessionHndlr.RequestURL,
                //                            0,
                //                             oSessionHndlr.FileMIMETypeDLL,
                //                            oSessionHndlr.RequestRawData.Length,
                //                            tempData,
                //                             string.Empty,
                //                            oSessionHndlr.ClientID,
                //                            oSessionHndlr.ClientName,
                //                            string.Empty);
                //}
                //else
                //{
                //    DBAnalyser.DataPacketsDB.InsertInDowloads(oSessionHndlr.StartedDateTime.ToString(ConstantVariables.DATETIMEFORMAT),
                //                                            oSessionHndlr.RequestURL,
                //                                            0,
                //                                            oSessionHndlr.FileMIMETypeDLL,
                //                                            oSessionHndlr.ResponseRawData.Length,
                //                                           tempData,
                //                                             string.Empty,
                //                                            oSessionHndlr.ClientID,
                //                                            oSessionHndlr.ClientName,
                //                                            string.Empty);
                //}
            }
        }
    }
}
