using Microsoft.Win32.SafeHandles;
using NibblePoker.Win32.Mailslot.Exceptions;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

using static NibblePoker.Win32.Mailslot.MailslotBindings;
using static NibblePoker.Win32.Mailslot.MailslotConstants;

namespace NibblePoker.Win32.Mailslot;

public class MailslotClient {

    /// <summary>
    /// UNC path to which the client is connected.<br/>
    /// Format: <c>\\{domain}\mailslot\{path}</c>
    /// </summary>
    public string FullPath {
        get;
        private set;
    }

    public bool IsAsync {
        get;
        private set;
    }

    public bool AutoFlushing {
        get;
        private set;
    }

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

    public FileStream GetFileStream(int bufferSize = 0) {
        return new FileStream(MailslotHandle, FileAccess.Write, bufferSize, IsAsync);
    }

    public static FileStream CreateAsFileStream(string domain, string path, int bufferSize = 0, bool isAsync = true) {
        return new MailslotClient(domain, path, isAsync).GetFileStream(bufferSize);
    }
}
