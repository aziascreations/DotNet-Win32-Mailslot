using Microsoft.Win32.SafeHandles;
using NibblePoker.Win32.Mailslot.Exceptions;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace NibblePoker.Win32.Mailslot;

public class MailslotClient : Mailslot {

    #region Constants

    /// <summary>
    /// ...
    /// </summary>
    public const int FILE_FLAG_NONE = 0;

    /// <summary>
    /// Waits forever for a message.
    /// </summary>
    public const int FILE_FLAG_OVERLAPPED = 0x40000000;

    #endregion

    public string FullPath {
        get;
        private set;
    }

    public bool IsAsync {
        get;
        private set;
    }

    public override uint MaxMessageSize => throw new NotImplementedException();

    internal SafeFileHandle MailslotHandle;

    public MailslotClient(string domain, string path, bool isAsync = true) {
        FullPath = $"\\\\{domain}\\mailslot\\{path}";
        if (!PathIsUNC(FullPath)) {
            throw new InvalidUncPathException(domain, "mailslot\\" + path);
        }

        IsAsync = isAsync;

        MailslotHandle = CreateFile(
            FullPath,
            FileAccess.Write,    // Clients can only write
            FileShare.ReadWrite, // Shared with the server
            IntPtr.Zero,         // Ignored for mailslots
            FileMode.Open,       // Same as `OPEN_EXISTING`
            IsAsync ? (FileAttributes) FILE_FLAG_OVERLAPPED : FILE_FLAG_NONE,
            IntPtr.Zero
        );
        if (MailslotHandle.IsInvalid) {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    public FileStream GetFileStream(int bufferSize = 4096) {
        return new FileStream(MailslotHandle, FileAccess.Write, bufferSize, IsAsync);
    }

    public static FileStream CreateAsFileStream(string domain, string path, int bufferSize = 4096, bool isAsync = true) {
        return new MailslotClient(domain, path, isAsync).GetFileStream(bufferSize);
    }


    #region PInvoke

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(
        [In] string lpFileName,
        [In] FileAccess dwDesiredAccess,
        [In] FileShare dwShareMode,
        [In, Optional] IntPtr SecurityAttributes,
        [In] FileMode dwCreationDisposition,
        [In] FileAttributes dwFlagsAndAttributes,
        [In, Optional] IntPtr hTemplateFile
    );

    #endregion
}
