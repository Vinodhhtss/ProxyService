using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO.Pipes;
using System.Threading;
using System.Security.AccessControl;
using System.Security.Principal;

namespace pipecomm
{
    struct PipeHeaderData
    {
        public PipeStream pipe;
        public Object state;
        public Byte[] data;
    };

    public interface IPipeHandler
    {
        void OnAsyncConnect(PipeStream pipe, out Object state);
        void OnAsyncDisconnect(PipeStream pipe, Object state);
        void OnAsyncMessage(PipeStream pipe, Byte[] data, Int32 bytes, Object state);
    }

    
    public class PipeServer
    {
        public const Int32 BUFFER_SIZE = 4096;

        private readonly String m_pipename;
        private readonly IPipeHandler m_handler;
        private readonly PipeSecurity m_sec;
        private bool m_running;

        private Dictionary<PipeStream, PipeHeaderData> m_pipes = new Dictionary<PipeStream, PipeHeaderData>();

        public PipeServer (String name, IPipeHandler handler, int instances)
       {
            m_running = true;

            m_handler = handler;
            m_pipename = name;


            m_sec = new PipeSecurity();
            m_sec.AddAccessRule(new PipeAccessRule(WindowsIdentity.GetCurrent().User,
                            PipeAccessRights.FullControl, AccessControlType.Allow)
            );

            m_sec.AddAccessRule(new PipeAccessRule(
                        new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),
                        PipeAccessRights.ReadWrite, AccessControlType.Allow)
            );

            for (int i = 0; i < instances; i++)
            {
                PipeServerCreate();
            }
        }

        public void PipeServerStop()
        {
            lock (m_pipes)
            {
                m_running = false;
                foreach(var pipe in m_pipes.Keys)
                {
                    pipe.Close();
                }
            }

            for (; ; )
            {
                int count;
                lock (m_pipes)
                {
                    count = m_pipes.Count;
                }
                if (count == 0)
                {
                    break;
                }
                Thread.Sleep(5);
            }
        }

        private void OnClientConnected(IAsyncResult result)
        {
            NamedPipeServerStream pipe = (NamedPipeServerStream)result.AsyncState;
            pipe.EndWaitForConnection(result);

            PipeHeaderData hd = new PipeHeaderData();
            hd.state = null;
            hd.pipe = pipe;
            hd.data = new Byte[BUFFER_SIZE];

            bool running;

            lock (m_pipes)
            {
                running = m_running;
                if (running)
                {
                    m_pipes.Add(hd.pipe, hd);
                }

            }

            if (running)
            {
                PipeServerCreate();

                m_handler.OnAsyncConnect(pipe, out hd.state);

                BeginRead(hd);
            }
            else {
                pipe.Close();
            }

        }

        private void PipeServerCreate()
        {
            NamedPipeServerStream pipe = new NamedPipeServerStream(
                    m_pipename,
                    PipeDirection.InOut,
                    -1,
                    PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous | PipeOptions.WriteThrough,
                    BUFFER_SIZE,
                    BUFFER_SIZE,
                    m_sec
                );

            pipe.BeginWaitForConnection(OnClientConnected, pipe);
        }

        private void BeginRead(PipeHeaderData hd)
        {
            bool isConnected = hd.pipe.IsConnected;
            if (isConnected)
            {
                try
                {
                    hd.pipe.BeginRead(hd.data, 0, hd.data.Length, OnAsyncMessage, hd);
                }
                catch (Exception)
                {
                    isConnected = false;
                }
            }

            if (!isConnected)
            {
                hd.pipe.Close();
                m_handler.OnAsyncDisconnect(hd.pipe, hd.state);
                lock (m_pipes)
                {
                    bool removed = m_pipes.Remove(hd.pipe);
                }
            }

        }

        private void OnAsyncMessage(IAsyncResult result)
        {
            PipeHeaderData hd = (PipeHeaderData)result.AsyncState;
            Int32 bytesRead = hd.pipe.EndRead(result);
            if (bytesRead != 0)
            {
                m_handler.OnAsyncMessage(hd.pipe, hd.data, bytesRead, hd.state);
            }
            BeginRead(hd);

        }
    }


    public class PipeClient
    {
        private readonly NamedPipeClientStream m_pipe;

        public PipeClient(string serverName, String pipeName)
        {
            m_pipe = new NamedPipeClientStream(
                serverName,
                pipeName,
                PipeDirection.InOut,
                PipeOptions.Asynchronous | PipeOptions.WriteThrough
            );
        }

        public PipeStream Connect(Int32 timeout)
        {
            m_pipe.Connect(timeout);

            m_pipe.ReadMode = PipeTransmissionMode.Message;

            return m_pipe;
        }

    }

}
