using System;
using System.IO;

namespace ProxyServiceAppln
{
    class Util
    {
        public static void CopyFilesAndFolders(string sourceDirName, string destPath)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                return;
            }
            DirectoryInfo[] dirs = dir.GetDirectories();


            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destPath, file.Name);
                file.CopyTo(temppath, true);
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destPath, subdir.Name);
                CopyFilesAndFolders(subdir.FullName, temppath);
            }
        }

        public static void SafeDeleteFile(string file)
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
            }
        }

        public static void SafeDeleteDirectory(string directory)
        {
            try
            {
                Directory.Delete(directory, true);
            }
            catch (Exception)
            {
            }
        }
    }
}
