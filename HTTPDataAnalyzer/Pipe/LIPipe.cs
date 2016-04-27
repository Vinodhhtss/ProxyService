using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.IO.Pipes;
using System.ComponentModel;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;
using System.Text;

namespace HTTPDataAnalyzer
{
    public static class LIPipe
    {
       
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern SafePipeHandle CreateNamedPipe(string lpName, uint dwOpenMode,
           uint dwPipeMode, int nMaxInstances, int nOutBufferSize, int nInBufferSize,
           uint nDefaultTimeOut, SECURITY_ATTRIBUTES lpSecurityAttributes);


        [DllImport("Advapi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = false)]
        private static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(
            [In] string StringSecurityDescriptor,
            [In] uint StringSDRevision,
            [Out] out IntPtr SecurityDescriptor,
            [Out] out int SecurityDescriptorSize
        );

        [StructLayout(LayoutKind.Sequential)]
        internal class SECURITY_ATTRIBUTES
        {
            public int nLength = 12;
            public IntPtr lpSecurityDescriptor = IntPtr.Zero;
            public bool bInheritHandle;
        }

        private const string LOW_INTEGRITY_SSL_SACL = "S:(ML;;NW;;;LW)";

        private static string CreateSddlForPipeSecurity()
        {
            const string LOW_INTEGRITY_LABEL_SACL = "S:(ML;;NW;;;LW)";
            const string EVERYONE_CLIENT_ACE = "(A;;0x12019b;;;WD)";
            const string CALLER_ACE_TEMPLATE = "(A;;0x12019f;;;{0})";

            StringBuilder sb = new StringBuilder();
            sb.Append(LOW_INTEGRITY_LABEL_SACL);
            sb.Append("D:");
            sb.Append(EVERYONE_CLIENT_ACE);
            sb.AppendFormat(CALLER_ACE_TEMPLATE, WindowsIdentity.GetCurrent().Owner.Value);
            return sb.ToString();
        }

        public static SafePipeHandle CreateLowIntegrityNamedPipe(string pipeName)
        {
            // convert the security descriptor
            IntPtr securityDescriptorPtr = IntPtr.Zero;
            int securityDescriptorSize = 0;
            bool result = ConvertStringSecurityDescriptorToSecurityDescriptor(
                CreateSddlForPipeSecurity(), 1, out securityDescriptorPtr, out securityDescriptorSize);
            if (!result)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            SECURITY_ATTRIBUTES securityAttributes = new SECURITY_ATTRIBUTES();
            securityAttributes.nLength = Marshal.SizeOf(securityAttributes);
            securityAttributes.bInheritHandle = true;
            securityAttributes.lpSecurityDescriptor =  securityDescriptorPtr;

            SafePipeHandle handle = CreateNamedPipe(@"\\.\pipe\" + pipeName,
                PipeDirection.InOut, 100, PipeTransmissionMode.Message, PipeOptions.Asynchronous,
                9192, 9192, PipeAccessRights.ReadWrite,(SECURITY_ATTRIBUTES)securityAttributes);
            
            if (handle.IsInvalid)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return handle;
        }

        private static SafePipeHandle CreateNamedPipe(string fullPipeName, PipeDirection direction,
            int maxNumberOfServerInstances, PipeTransmissionMode transmissionMode, PipeOptions options,
            int inBufferSize, int outBufferSize, PipeAccessRights rights, SECURITY_ATTRIBUTES secAttrs)
        {
            uint openMode = (uint)direction | (uint)options;
            uint pipeMode = 0x4; //Message Mode
            if (maxNumberOfServerInstances == -1)
                maxNumberOfServerInstances = 0xff;

            SafePipeHandle handle = CreateNamedPipe(fullPipeName, openMode, pipeMode,
                maxNumberOfServerInstances, outBufferSize, inBufferSize, 0, secAttrs);
            if (handle.IsInvalid)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return handle;
        }

    }
}