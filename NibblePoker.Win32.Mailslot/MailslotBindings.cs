using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace NibblePoker.Win32.Mailslot;

internal static class MailslotBindings {
    [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern SafeFileHandle CreateFile(
        [In] string lpFileName,
        [In] FileAccess dwDesiredAccess,
        [In] FileShare dwShareMode,
        [In, Optional] IntPtr SecurityAttributes,
        [In] FileMode dwCreationDisposition,
        [In] FileAttributes dwFlagsAndAttributes,
        [In, Optional] IntPtr hTemplateFile
    );

    [DllImport("Shlwapi.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = false)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool PathIsUNC(
        [In] string pszPath
    );

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern SafeFileHandle CreateMailslot(
        [In] string lpName,
        [In] uint nMaxMessageSize,
        [In] uint lReadTimeout,
        [In, Optional] IntPtr lpSecurityAttributes
    );

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.None, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetMailslotInfo(
        [In] SafeHandle hMailslot,
        [Out, Optional] out uint? lpMaxMessageSize,
        [Out, Optional] out uint? lpNextSize,
        [Out, Optional] out uint? lpMessageCount,
        [Out, Optional] out uint? lpReadTimeout
    );

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.None, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetMailslotInfo(
      [In] SafeHandle hMailslot,
      [In] uint lReadTimeout
    );
}
