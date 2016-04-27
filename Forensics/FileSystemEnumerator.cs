using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Forensics
{
    public static class FileSystemEnumerator
    {

        public static void EnumerateFilesOfSize(UInt32 size, String rootdir)
        {

            DirectoryInfo diTop = new DirectoryInfo(rootdir);
            UInt32 index = 1;
            try
            {
                foreach (var fi in diTop.EnumerateFiles())
                {
                    try
                    {
                        if (fi.Length == size)
                        {
                            Console.WriteLine(fi.FullName);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        //Console.WriteLine("{0}", UnAuthTop.Message);
                    }
                }

                foreach (var di in diTop.EnumerateDirectories("*"))
                {
                    try
                    {
                        foreach (var fi in di.EnumerateFiles("*", SearchOption.AllDirectories))
                        {
                            try
                            {
                                // Display each file over 10 MB; 
                                if (fi.Length == size)
                                {
                                    Console.WriteLine("{0}\t\t{1}", fi.FullName, fi.Length.ToString("N0"));
                                }
                            }
                            catch (UnauthorizedAccessException UnAuthFile)
                            {
                                Console.WriteLine("UnAuthFile: {0}", UnAuthFile.Message);
                            }
                        }
                    }
                    catch (UnauthorizedAccessException UnAuthSubDir)
                    {
                        //Console.WriteLine("UnAuthSubDir: {0}", UnAuthSubDir.Message);
                    }
                }
            }
            catch (DirectoryNotFoundException DirNotFound)
            {
                Console.WriteLine("{0}", DirNotFound.Message);
            }
            catch (UnauthorizedAccessException UnAuthDir)
            {
                Console.WriteLine("UnAuthDir: {0}", UnAuthDir.Message);
            }
            catch (PathTooLongException LongPath)
            {
                Console.WriteLine("{0}", LongPath.Message);
            }


        }
    }
}
