using System;
using System.Diagnostics;
using System.IO.Pipes;

namespace HTTPProxyServer
{
    public class PipeClient
    {
        public NamedPipeClientStream m_pipe;

        public PipeClient(string serverName, String pipeName)
        {
            m_pipe = new NamedPipeClientStream(
                serverName,
                pipeName,
                PipeDirection.Out,
                PipeOptions.Asynchronous | PipeOptions.WriteThrough
            );
        }

        public PipeStream Connect(Int32 timeout)
        {
            try
            {
                m_pipe.Connect(timeout);
                m_pipe.ReadMode = PipeTransmissionMode.Message;
            }
            catch (Exception)
            {
                throw;
            }
            return m_pipe;
        }
    }
}
