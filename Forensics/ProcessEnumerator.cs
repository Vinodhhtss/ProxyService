using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Forensics
{
    public class ProcessEnumerator
    {
        public bool ProcessPathFromPid(out string path, uint pid)
        {
            path = String.Empty;

            if (pid == 0)
            {
                return false;
            }

            try
            {
                path = Process.GetProcessById((int)pid).MainModule.FileName;
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
