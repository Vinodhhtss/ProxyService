using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Pipes;
using System.Threading;
using System.Windows.Forms;

namespace pipecomm
{
    public class SecurePipeServer : pipecomm.IPipeHandler
    {
        private pipecomm.PipeServer m_srv;
        private Int32 m_count;

        public String m_pipename;
        public int m_instcount;

        public bool Init (string pipename, int instance)
        {
            if (pipename == string.Empty || instance <= 0 || instance >= 11)
                return false;

            m_pipename = pipename;
            m_instcount = instance;

            return true;
        }

        public void Start()
        {
            m_srv = new pipecomm.PipeServer(m_pipename, this, m_instcount);
        }

        public void Stop()
        {
            m_srv.PipeServerStop();
        }

        public void OnAsyncConnect(PipeStream pipe, out Object state)
        {
            Int32 count = Interlocked.Increment(ref m_count);
            Console.WriteLine("Connected : " + count);
            state = count;
        }

        public void OnAsyncDisconnect(PipeStream pipe, Object state)
        {
            Console.WriteLine("Disconnected :" + (Int32)state);
        }

        public void OnAsyncMessage(PipeStream pipe, Byte[] data, Int32 bytes, Object state)
        {
            data = Encoding.UTF8.GetBytes(Encoding.UTF8.GetString(data, 0, bytes).ToUpper().ToCharArray());

            try
            {
                MessageBox.Show(Encoding.ASCII.GetString(data, 0, bytes));

                pipe.BeginWrite(data, 0, bytes, OnAsyncWriteComplete, pipe);
            }
            catch (Exception)
            {
                pipe.Close();
            }
        }

        public void OnAsyncWriteComplete(IAsyncResult result)
        {
            PipeStream pipe = (PipeStream)result.AsyncState;
            pipe.EndWrite(result);
        }

    }
}
