using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO.Pipes;

namespace pipecomm
{
    public class SecurePipeClient
    {
        String m_serverpipe;

        public bool Init(string serverpipe)
        {
            if (serverpipe == string.Empty)
            {
                return false;
            }

            m_serverpipe = serverpipe;

            return true;
        }

        public bool SendMessage(string message, out string response)
        {
            response = string.Empty;

            pipecomm.PipeClient cli = new pipecomm.PipeClient(".", m_serverpipe);
            PipeStream pipe;
            try
            {
                pipe = cli.Connect(10);
            }
            catch (Exception)
            {
                return false;
            }

            Byte[] sendmsg = Encoding.UTF8.GetBytes(message);
            pipe.Write(sendmsg, 0, sendmsg.Length);

            Byte[] data = new Byte[4096];
            Int32 bytesRead = pipe.Read(data, 0, data.Length);

            response = Encoding.UTF8.GetString(data, 0, bytesRead);

            return true;
        }
    }
}
