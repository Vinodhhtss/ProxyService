namespace HTTPDataAnalyzer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;

    public class FileUtil
    {
        public static string GetSuggestedFileName(string fileName)
        {
            int count = 1;

            string returnFileName = fileName;
            string folderName = Path.GetDirectoryName(fileName);

            if (!File.Exists(fileName))
            {
                return fileName;
            }
            else
            {
                string fileNameWithoutEx = string.Empty;
                string fileExtension = string.Empty;

                while (File.Exists(returnFileName))
                {
                    try
                    {
                        fileNameWithoutEx = Path.GetFileNameWithoutExtension(fileName);
                    }
                    catch (Exception ex)
                    {
                        //AnalyzerManager.Logger.Error(ex);
                    }

                    try
                    {
                        fileExtension = Path.GetExtension(fileName);
                    }
                    catch (Exception ex)
                    {
                        //AnalyzerManager.Logger.Error(ex);
                    }

                    returnFileName = fileNameWithoutEx + '(' + count + ')';
                    returnFileName = Path.Combine(folderName, returnFileName);
                    returnFileName += fileExtension;
                    count++;
                }
            }
            return returnFileName;
        }

        public static string TrimFileName(string trimString)
        {
            trimString = trimString.Replace("=?UTF-8?Q?", string.Empty).Replace("?=", string.Empty).Replace("\"", String.Empty);

            if (trimString.IndexOf(";") > 0)
            {
                return trimString.Substring(0, trimString.IndexOf(";"));
            }
            else
            {
                return trimString;
            }
        }
    }
}
