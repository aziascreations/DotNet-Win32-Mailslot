using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using static NibblePoker.Win32.Mailslot.MailslotBindings;
using static NibblePoker.Win32.Mailslot.MailslotConstants;

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

    // Not set to private to enable some Win32 API based tests.
    internal SafeFileHandle MailslotHandle;

    private bool _disposed = false;
    private bool _ownsHandle;


    #region Constructors

    internal MailslotClient(string fullUncPath, bool isAsync, bool mustExist, bool ownsHandle) {
        FullPath = fullUncPath;
        if (!PathIsUNC(FullPath)) {
            throw new ArgumentException("Invalid combination of UNC host and path values !");
        }

        if (mustExist) {
            if (!File.Exists(FullPath)) {
                throw new IOException($"The resource at '{FullPath}' doesn't exist !");
            }
        }

        _ownsHandle = ownsHandle;

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

    internal MailslotClient(string host, string mailslotPath, bool isAsync, bool mustExist, bool ownsHandle) :
        this($"\\\\{host}\\mailslot\\{mailslotPath}", isAsync, mustExist, ownsHandle) { }

    /// <summary>
    ///     Creates a mailslot client connected to the given UNC path.
    /// </summary>
    /// <param name="fullUncPath">
    ///     Remote mailslot server's full UNC path.<br/>
    ///     Format: <c>\\{host}\mailslot\{path}</c>
    /// </param>
    /// <param name="isAsync"></param>
    /// <param name="mustExist">
    ///     Checks if the mailslot you specified exist before we attempt to connect to it.<br/>
    ///     Default: <c>true</c>
    /// </param>
    /// <exception cref="ArgumentException">
    ///     Thrown if the given UNC path is invalid.
    /// </exception>
    /// <exception cref="IOException">
    ///     Thrown if you specified or left <c>mustExist</c> as <c>true</c> and the
    ///     server doesn't exist or can't be connected to.
    /// </exception>
    /// <exception cref="Win32Exception">
    ///     Thrown if we got an invalid <see cref="SafeFileHandle"/> while opening
    ///     the mailslot in the Win32 APIs.
    /// </exception>
    /// <remarks>
    ///     Checking if a mailslot exist on a remote computer may slow down the program's
    ///     execution on extremely slow networks since we may end up doing the check twice.
    /// </remarks>
    public MailslotClient(string fullUncPath, bool isAsync = true, bool mustExist = true) :
        this(fullUncPath, isAsync, mustExist, ownsHandle: true) { }

    /// <summary>
    ///     Creates a mailslot client connected to the given host's mailslot at the given path.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="mailslotPath"></param>
    /// <param name="isAsync"></param>
    /// <param name="mustExist">
    ///     Checks if the mailslot you specified exist before we attempt to connect to it.<br/>
    ///     Default: <c>true</c>
    /// </param>
    /// <exception cref="ArgumentException">
    ///     Thrown if the given UNC path is invalid.
    /// </exception>
    /// <exception cref="IOException">
    ///     Thrown if you specified or left <c>mustExist</c> as <c>true</c> and the
    ///     server doesn't exist or can't be connected to.
    /// </exception>
    /// <exception cref="Win32Exception">
    ///     Thrown if we got an invalid <see cref="SafeFileHandle"/> while opening
    ///     the mailslot in the Win32 APIs.
    /// </exception>
    /// <remarks>
    ///     Checking if a mailslot exist on a remote computer may slow down the program's
    ///     execution on extremely slow networks since we may end up doing the check twice.
    /// </remarks>
    public MailslotClient(string host, string mailslotPath, bool isAsync = true, bool mustExist = true) :
        this($"\\\\{host}\\mailslot\\{mailslotPath}", isAsync, mustExist, ownsHandle: true) { }

    #endregion


    /// <summary>
    ///     Sends a given bit of text to the mailslot server.
    /// </summary>
    /// <param name="textToSend">
    ///     The bit of text to send.
    /// </param>
    /// <param name="encoding">
    ///     Encoding to use to convert the given string into bytes.
    ///     If left as <c>null</c>, we'll use <see cref="Encoding.Default"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the data to send is <c>null</c>.
    /// </exception>
    /// <remarks>
    ///     This function will not handles cases where the server allows a smaller
    ///      message than what you're sending.<br/>
    ///     You should implement support for this behavious on your own if needed.
    /// </remarks>
    public void Send(string textToSend, Encoding? encoding) {
        if (textToSend == null) {
            throw new ArgumentNullException(nameof(textToSend), "Unable to send a `null` bit of text !");
        }

        if (encoding == null) {
            encoding = Encoding.Default;
        }

        Send(encoding.GetBytes(textToSend));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="textToSend">
    ///     The byte buffer to send.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the data to send is <c>null</c>.
    /// </exception>
    /// <remarks>
    ///     This function will not handles cases where the server allows a smaller
    ///      message than what you're sending.<br/>
    ///     You should implement support for this behavious on your own if needed.
    /// </remarks>
    public void Send(byte[] dataToSend) {
        if (dataToSend == null) {
            throw new ArgumentNullException(nameof(dataToSend), "Unable to send a `null` byte buffer !");
        }

        using FileStream client = this.GetFileStream();
        client.Write(dataToSend, 0, dataToSend.Length);
        client.Flush();
    }

    /// <inheritdoc/>
    public void Dispose() {
        if (_disposed) {
            return;
        }
        _disposed = true;

        if (_ownsHandle) {
            if (!MailslotHandle.IsInvalid && !MailslotHandle.IsClosed) {
                MailslotHandle.Dispose();
            }
        }

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
    /// <remarks>
    ///     The returned FileStream doesn't own the <see cref="SafeFileHandle"/>, the <see cref="MailslotClient"/> instance does.
    /// </remarks>
    public FileStream GetFileStream(int bufferSize = 4096) {
        return new FileStream(
             new SafeFileHandle(MailslotHandle.DangerousGetHandle(), ownsHandle: false),
            FileAccess.Write, bufferSize, IsAsync
        );
    }


    #region Class methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fullUncPath"></param>
    /// <param name="bufferSize">
    ///     A positive <see cref="Int32"/> value greater than 0 indicating the buffer size.<br/>
    ///     The default buffer size is 4096.
    /// </param>
    /// <param name="isAsync"></param>
    /// <param name="mustExist">
    ///     Checks if the mailslot you specified exist before we attempt to connect to it.<br/>
    ///     Default: <c>true</c>
    /// </param>
    /// <returns></returns>
    public static FileStream CreateAsFileStream(string fullUncPath, int bufferSize = 4096,
                                                bool isAsync = true, bool mustExist = true) {

        using var ms = new MailslotClient(fullUncPath, isAsync, mustExist, ownsHandle: false);

        // Creates a FileStream that will take ownership of the SafeFileHandle.
        // It won't get closed in `MailslotClient.Dispose`.
        return new FileStream(
            ms.MailslotHandle,
            FileAccess.Write, bufferSize, true
        );


    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="host"></param>
    /// <param name="mailslotPath"></param>
    /// <param name="bufferSize">
    ///     A positive <see cref="Int32"/> value greater than 0 indicating the buffer size.<br/>
    ///     The default buffer size is 4096.
    /// </param>
    /// <param name="isAsync"></param>
    /// <param name="mustExist">
    ///     Checks if the mailslot you specified exist before we attempt to connect to it.<br/>
    ///     Default: <c>true</c>
    /// </param>
    /// <returns></returns>
    public static FileStream CreateAsFileStream(string host, string mailslotPath, int bufferSize = 4096,
                                                bool isAsync = true, bool mustExist = true) {

        using var ms = new MailslotClient(host, mailslotPath, isAsync, mustExist, ownsHandle: false);

        // Creates a FileStream that will take ownership of the SafeFileHandle.
        // It won't get closed in `MailslotClient.Dispose`.
        return new FileStream(
            ms.MailslotHandle,
            FileAccess.Write, bufferSize, true
        );
    }

    #endregion

}
