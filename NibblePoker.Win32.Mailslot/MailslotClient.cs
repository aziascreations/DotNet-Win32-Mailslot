using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using static NibblePoker.Win32.Mailslot.MailslotBindings;
using static NibblePoker.Win32.Mailslot.MailslotConstants;
using static NibblePoker.Win32.Mailslot.MailslotUtils;

#pragma warning disable IDE0074 // Use compound assignment

namespace NibblePoker.Win32.Mailslot;

/// <summary>
/// Represents a mailslot server and provides all the utilities related to them.
/// </summary>
public class MailslotClient : IDisposable {

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

    internal SafeFileHandle MailslotHandle;

    public MailslotClient(string host, string path, bool isAsync = true, bool mustExist = true) {
        if (!IsValidUNCHost(host)) {
            throw new ArgumentException("Invalid UNC host value !", nameof(host));
        }
        if (!IsValidUNCPath(path)) {
            throw new ArgumentException("Invalid UNC path value !", nameof(path));
        }

        FullPath = $"\\\\{host}\\mailslot\\{path}";
        if (!PathIsUNC(FullPath)) {
            throw new ArgumentException("Invalid combination of UNC host and path values !");
        }

        if (mustExist) {
            if (!File.Exists(FullPath)) {
                throw new IOException($"The resource at '{FullPath}' doesn't exist !");
            }
        }

        IsAsync = isAsync;

        MailslotHandle = CreateFile(
            FullPath,
            FileAccess.Write,    // Clients can only write
            FileShare.ReadWrite, // Shared with the server
            IntPtr.Zero,         // Ignored for mailslots
            FileMode.Open,       // Same as `OPEN_EXISTING`
            IsAsync ? (FileAttributes) FILE_FLAG_OVERLAPPED : FILE_FLAG_NONE,
            IntPtr.Zero          // Ignored for mailslots
        );
        if (MailslotHandle.IsInvalid) {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="textToSend"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public bool Send(string textToSend, Encoding? encoding) {
        if (encoding == null) {
            encoding = Encoding.Default;
        }

        return Send(encoding.GetBytes(textToSend));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataToSend"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public bool Send(byte[] dataToSend) {
        if (dataToSend == null) {
            throw new NullReferenceException("...");
        }

        using FileStream client = this.GetFileStream();
        client.Write(dataToSend, 0, dataToSend.Length);
        client.Flush();

        return true;
    }

    public void Dispose() {
        if (MailslotHandle.IsInvalid) {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        MailslotHandle.Close();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bufferSize">
    ///     A positive <see cref="Int32"/> value greater than 0 indicating the buffer size.<br/>
    ///     The default buffer size is 4096.
    /// </param>
    /// <returns></returns>
    public FileStream GetFileStream(int bufferSize = 4096) {
        return new FileStream(MailslotHandle, FileAccess.Write, bufferSize, IsAsync);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="domain"></param>
    /// <param name="path"></param>
    /// <param name="bufferSize">
    ///     A positive <see cref="Int32"/> value greater than 0 indicating the buffer size.<br/>
    ///     The default buffer size is 4096.
    /// </param>
    /// <param name="isAsync"></param>
    /// <returns></returns>
    public static FileStream CreateAsFileStream(string domain, string path, int bufferSize = 4096, bool isAsync = true) {
        return new MailslotClient(domain, path, isAsync).GetFileStream(bufferSize);
    }
}
