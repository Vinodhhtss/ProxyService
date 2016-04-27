using System;
using System.IO;

namespace ProxyServiceDBAppln
{
    class Util
    {
        public static void CopyFilesAndFolders(string sourceDirName, string destPath)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                return;
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destPath, file.Name);

                // Copy the file.
                file.CopyTo(temppath, true);
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                // Create the subdirectory.
                string temppath = Path.Combine(destPath, subdir.Name);

                // Copy the subdirectories.
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

        public static void SafeDeleteDirectory()
        {
            string pathtodelete = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            try
            {
                Directory.Delete(pathtodelete, true);
            }
            catch (Exception)
            {
            }
        }
    }
}
