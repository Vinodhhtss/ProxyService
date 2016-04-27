using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;

namespace HTTPDataAnalyzer
{
    class PipeHeaderData
    {
        public PipeStream pipe;
        public Object state;
        public Byte[] data;
        public int pipeServerType;
        public PipeHeaderData(int serverType)
        {
            pipeServerType = serverType;
        }
    };

    public interface IPipeHandler
    {
        void OnConnect(PipeStream pipe, out Object state);
        void OnDisconnect(PipeStream pipe, Object state);
    }

    public class PipeServer
    {
        public const Int32 BUFFER_SIZE = 9192;

        private readonly String m_pipename;
        private readonly IPipeHandler m_handler;
        private readonly PipeSecurity m_sec;
        private int m_ServerInstanceCount = 0;

        private ConcurrentDictionary<PipeStream, PipeHeaderData> m_pipes = new ConcurrentDictionary<PipeStream, PipeHeaderData>();

        public PipeServer(String name, IPipeHandler handler, int instances, int pipeServerType)
        {

            m_handler = handler;
            m_pipename = name;
            m_ServerInstanceCount = instances;

            switch (pipeServerType)
            {
                case 0:
                    m_sec = new PipeSecurity();
                    m_sec.AddAccessRule(new PipeAccessRule(WindowsIdentity.GetCurrent().User,
                                    PipeAccessRights.FullControl, AccessControlType.Allow)
                    );

                    m_sec.AddAccessRule(new PipeAccessRule(
                                new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),
                                PipeAccessRights.ReadWrite, AccessControlType.Allow)
                    );
                    break;

                default:
                    break;
            }

            for (int i = 0; i < instances; i++)
            {
                PipeServerCreate(pipeServerType);
            }
        }

        private void OnClientConnected(IAsyncResult result)
        {
            NamedPipeServerStream pipe = (NamedPipeServerStream)result.AsyncState;
            pipe.EndWaitForConnection(result);

            PipeHeaderData hd = m_pipes[pipe];
            hd.state = null;
            hd.pipe = pipe;
            hd.data = new byte[BUFFER_SIZE];

            m_handler.OnConnect(pipe, out hd.state);

            BeginRead(hd);

        }

        private void PipeServerCreate(int pipeServerType)
        {
            try
            {

                NamedPipeServerStream pipe = null;
                switch (pipeServerType)
                {
                    case 0:
                        pipe = new NamedPipeServerStream(
                            m_pipename,
                            PipeDirection.In,
                            m_ServerInstanceCount,
                            PipeTransmissionMode.Message,
                            PipeOptions.Asynchronous | PipeOptions.WriteThrough,
                            BUFFER_SIZE,
                            BUFFER_SIZE,
                            m_sec
                        );
                        break;
                    case 1:
                        SafePipeHandle handle = LIPipe.CreateLowIntegrityNamedPipe(m_pipename);
                        pipe = new NamedPipeServerStream(PipeDirection.InOut,
                                                                true,
                                                                false,
                                                                (SafePipeHandle)handle);

                        pipe.ReadMode = PipeTransmissionMode.Message;

                        break;
                    default:
                        break;
                }
                m_pipes.TryAdd(pipe, new PipeHeaderData(pipeServerType));
                pipe.BeginWaitForConnection(OnClientConnected, pipe);
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
            }
        }

        private void BeginRead(PipeHeaderData hd)
        {
            bool isConnected = hd.pipe.IsConnected;

            try
            {
                hd.pipe.BeginRead(hd.data, 0, hd.data.Length, OnReadMessage, hd);
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
                isConnected = false;
            }
        }

        private void OnReadMessage(IAsyncResult result)
        {
            PipeHeaderData hd = (PipeHeaderData)result.AsyncState;
            int bytesRead = hd.pipe.EndRead(result);
            if (bytesRead != 0)
            {
                Array.Resize(ref hd.data, bytesRead);
                try
                {
                    switch (hd.pipeServerType)
                    {
                        case 0:
                            MessageProcessor.ProcessMessage(Encoding.ASCII.GetString(hd.data));
                            break;
                        case 1:
                            HookDllMessageProcessor.ProcessMessage(hd.data);
                            break;
                        default:
                            break;

                    }
                }
                catch (Exception ex)
                {
                    //AnalyzerManager.Logger.Error(ex);
                }
                BeginRead(hd);
            }
            else
            {
                try
                {
                    m_pipes.TryRemove(hd.pipe, out hd);
                    hd.pipe.Close();
                }
                catch (Exception ex)
                {
                    //AnalyzerManager.Logger.Error(ex);
                }
                if (hd != null)
                {
                    m_handler.OnDisconnect(hd.pipe, hd.state);
                    PipeServerCreate(hd.pipeServerType);
                }
            }
        }

        public void PipeServerStop()
        {
            try
            {
                foreach (var pipe in m_pipes.Keys)
                {
                    pipe.Close();
                }
            }
            catch (Exception ex)
            {
                //AnalyzerManager.Logger.Error(ex);
            }
        }
    }
}
