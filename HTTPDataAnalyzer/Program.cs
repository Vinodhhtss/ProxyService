using HTTPDataAnalyzer.Registration;
using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace HTTPDataAnalyzer
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //Util.Export(string.Empty, string.Empty);
            //return;
           Util.GetWindowsUserSID();
            AnalyzerManager.Analyzer = new AnalyzerManager();
            Console.ReadLine();
        }
    }
}
