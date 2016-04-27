using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace HTTPDataAnalyzer
{
    public class StartPipeServer
    {
        static SecurePipeServer securePipe = null;
        static SecurePipeServer secureDNSPipe = null;

        public static void StartServer()
        {
            securePipe = new SecurePipeServer(ConstantVariables.PIPE_SERVER_NAME, 5);
            securePipe.Start(0);
            secureDNSPipe = new SecurePipeServer(ConstantVariables.PIPE_DNS_SERVER_NAME, 5);
            secureDNSPipe.Start(1);
        }

        public static void StopServer()
        {
            if (securePipe != null)
            {
                securePipe.Stop();
            }
            if (secureDNSPipe != null)
            {
                secureDNSPipe.Stop();
            }
        }
    }
}
