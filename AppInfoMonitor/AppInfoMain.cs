using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace AppInfoMonitor
{


    class AppInfoMain
    {

        #region prototype

        public delegate void WaitOrTimerDelegate(IntPtr lpParameter, bool TimerOrWaitFired);

        [DllImport("kernel32.dll")]
        static extern bool RegisterWaitForSingleObject(out IntPtr phNewWaitObject,
           IntPtr hObject, WaitOrTimerDelegate Callback, IntPtr Context,
           uint dwMilliseconds, uint dwFlags);

        [DllImport("kernel32.dll")]
        static extern void ExitProcess(uint uExitCode);

        #endregion

        #region Win32 Structs

        private static uint INFINITE = 0xFFFFFFFF;

        public enum ExecuteFlags
        {
            WT_EXECUTEDEFAULT = 0x00000000,

            WT_EXECUTEINTIMERTHREAD = 0x00000020,

            WT_EXECUTEINIOTHREAD = 0x00000001,

            WT_EXECUTEINWAITTHREAD = 0x00000004,

            WT_EXECUTEINPERSISTENTTHREAD = 0x00000080,

            WT_EXECUTELONGFUNCTION = 0x00000010,

            WT_EXECUTEONLYONCE = 0x00000008,

            WT_TRANSFER_IMPERSONATION = 0x00000100
        };

        #endregion


        static bool m_parentexit = false;

        static int m_parentpid = 0;

        static pipecomm.SecurePipeServer srv = null;

        static void ParentDied(IntPtr lpParameter, bool TimerOrWaitFired)
        {
            //ExitProcess(0);

        }

        static void MonitorParentThread()
        {
            ///// Name of the parent process
            string procname = "testservice";

            while (true)
            {
                Thread.Sleep(2000);
                //continue;
                if (m_parentpid > 0)
                {
                    try
                    {
                        using (Process localproc = Process.GetProcessById(m_parentpid))
                        {
                            
                            if (localproc != null)
                            {
                                /*
                                if (String.Equals(localproc.ProcessName, procname,
                                       StringComparison.CurrentCultureIgnoreCase) == true)
                                {
                                    continue;
                                }
                                else
                                {
                                    continue;
                                    //////// Right now just ignore
                                    // MessageBox.Show(localproc.ProcessName);
                                    // m_parentexit = true;
                                    // break;
                                }*/
                                
                            }
                            else
                            {
                                //MessageBox.Show(" B " + localproc.ProcessName);
                                m_parentexit = true;
                                break;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        m_parentexit = true;
                        break;
                    }
                }

            }

        }        

        static bool SetupPipeServer()
        {
            try
            {
                srv = new pipecomm.SecurePipeServer();
                if (srv.Init("T1Pipe", 5) == false)
                {
                    //MessageBox.Show("SRV INIT Error");
                    return false;
                }

                srv.Start();

                return true;
            } 
            catch(Exception)
            {
            
            }
            return false;
        }

        static void Main(string[] args)
        {
            if (args.Length == 1) { 
              

                try {

                   int  Parentpid = int.Parse(args[0]);

                   m_parentpid = Parentpid;

                   Thread m_thread = new Thread(MonitorParentThread);
                   m_thread.Start();

                   if (SetupPipeServer() == false)
                   {
                       return;
                   }

               
                   m_thread.Join();

                   if (m_parentexit == true)
                   {
                       //MessageBox.Show("Parent Exited...");
                   }
                   else
                   {
                      // MessageBox.Show("Parent Exit Fail..");
                   }

                   srv.Stop();

                } 
                catch (Exception ex)
                {
                   // MessageBox.Show(ex.Message);
                }


                
            }
            else {
                ;
            }
        }

        private static void Parent_Exited(object sender, System.EventArgs e)
        {
            m_parentexit = true;
            //MessageBox.Show("Parent Exited");
        }


    }
}
