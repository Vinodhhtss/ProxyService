using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SHDocVw;
using System.Runtime.InteropServices;

namespace Forensics
{
    public class IEOpenUrls
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public bool GetCurrentUrls(out List<string> urls)
        {
            //SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindowsClass();
            urls = new List<string>();

            foreach (InternetExplorer ie in new ShellWindows())
            {
               
                IntPtr windowval = (IntPtr)ie.HWND;
                uint pid = 0;
                if (windowval != null )
                {
                    GetWindowThreadProcessId(windowval, out pid);

                }
                Console.WriteLine("ie.LocationURL: " + ie.LocationURL + " : Pid is  :" + pid);


                urls.Add(ie.LocationURL);               
            }

            return true;
        }
    }

    public class OfficeOpenUrls
    {
        public bool GetCurrentWordDocs(out List<string> docs)
        {
             docs = new List<string>();
             Microsoft.Office.Interop.Word.Application WordObj = null;
            try
            {
                
                WordObj = (Microsoft.Office.Interop.Word.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Word.Application");
                for (int i = 0; i < WordObj.Windows.Count; i++)
                {
                    object idx = i + 1;
                    Microsoft.Office.Interop.Word.Window WinObj = WordObj.Windows.get_Item(ref idx);
                    docs.Add(WinObj.Document.FullName);          
                }
            }
            catch (Exception )
            {

            }
            finally 
            {
                if (WordObj != null)
                {
                    Marshal.ReleaseComObject(WordObj);
                }
            }

            return true;
        }

        public bool GetCurrentExcelDocs(out List<string> ws)
        {
            ws = new List<string>();
            Microsoft.Office.Interop.Excel.Application ExcelObj = null;
            try
            {
               
                ExcelObj = (Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");


                Console.WriteLine("Open : " + ExcelObj.Workbooks.Count);

                foreach (Microsoft.Office.Interop.Excel.Workbook wb in ExcelObj.Workbooks)
                {
                        Console.WriteLine("Open : " + wb.FullName);
                        ws.Add(wb.FullName);
                }
          
            }
            catch (Exception)
            {

            }

            return true;
        }

        public bool GetCurrentPowepointDocs(out List<string> ppt)
        {
            ppt = new List<string>();
            Microsoft.Office.Interop.PowerPoint.Application PpObj = null;
            try
            {
                
                PpObj = (Microsoft.Office.Interop.PowerPoint.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Powerpoint.Application");


                Console.WriteLine("Open : " + PpObj.Windows.Count);
                foreach (Microsoft.Office.Interop.PowerPoint.Presentation pp in PpObj.Presentations)
                {
                    Console.WriteLine("Open : " + pp.FullName);
                    ppt.Add(pp.FullName);
                }

            }
            catch (Exception)
            {

            }
            finally
            {
                if (PpObj != null)
                {
                    Marshal.ReleaseComObject(PpObj);
                }
            }

            return true;
        }


    }
}
