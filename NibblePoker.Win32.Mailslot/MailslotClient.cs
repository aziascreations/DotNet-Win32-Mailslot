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
/// Represents a mailslot client and provides all the utilities related to it.
/// </summary>
/// <remarks>
///     This class <b>is</b> the <see cref="FileStream"/> you write to: there's no separate
///      handle to own, duplicate or hand out. Just pass the instance itself around to
///      wherever it needs to be written to.
/// </remarks>
public class MailslotClient : FileStream {

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
    // `SafeFileHandle` is inherited from `FileStream`: this instance's handle *is* the mailslot's handle.
    internal SafeFileHandle MailslotHandle {
        get => SafeFileHandle;
    }


    #region Constructors

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
    /// <param name="bufferSize">
    ///     A positive <see cref="Int32"/> value greater than 0 indicating the buffer size.<br/>
    ///     The default buffer size is 4096.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     Thrown if the given UNC path is invalid.<br/>
    ///     It gets thrown by the constructor and <see cref="MailslotUtils.ComposeMailslotUncPath"/>.
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
    /// <remarks>
    ///     This instance is itself the <see cref="FileStream"/> backed by the mailslot's
    ///      handle: it owns that handle for its whole lifetime, and disposing it closes it.
    ///      Pass the instance around directly wherever it needs to be written to.
    /// </remarks>
    public MailslotClient(string fullUncPath, bool isAsync = true, bool mustExist = true, int bufferSize = 4096) :
        base(CreateMailslotClientHandle(fullUncPath, isAsync, mustExist), FileAccess.Write, bufferSize, isAsync) {
        FullPath = fullUncPath;
        IsAsync = isAsync;
    }

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
    /// <param name="bufferSize">
    ///     A positive <see cref="Int32"/> value greater than 0 indicating the buffer size.<br/>
    ///     The default buffer size is 4096.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     Thrown if the given UNC path is invalid.<br/>
    ///     It gets thrown by the constructor and <see cref="MailslotUtils.ComposeMailslotUncPath"/>.
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
    /// <remarks>
    ///     This instance is itself the <see cref="FileStream"/> backed by the mailslot's
    ///      handle: it owns that handle for its whole lifetime, and disposing it closes it.
    ///      Pass the instance around directly wherever it needs to be written to.
    /// </remarks>
    public MailslotClient(string? host, string mailslotPath, bool isAsync = true, bool mustExist = true, int bufferSize = 4096) :
        this(
            MailslotUtils.ComposeMailslotUncPath(host != null ? host : ".", mailslotPath, false),
            isAsync, mustExist, bufferSize
        ) { }

    #endregion


    /// <summary>
    ///
    /// </summary>
    /// <param name="dataToSend">
    ///     The byte buffer to send.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the data to send is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when used on an async client.
    /// </exception>
    /// <exception cref="Win32Exception">
    ///     If there was an error while writing to the mailslot.
    /// </exception>
    /// <remarks>
    ///     This function will not handles cases where the server allows a smaller
    ///      message than what you're sending.<br/>
    ///     You should implement support for this behaviour on your own if needed.
    /// </remarks>
    /// <remarks>
    ///     This function doens't handle async <see cref="MailslotClient"/> since
    ///      we're not managing complex structures for Win32 API calls yet.
    /// </remarks>
    /*public void SendSync(byte[] dataToSend) {
        if (IsAsync) {
            throw new InvalidOperationException("Use Write/WriteAsync directly on this instance for async clients.");
        }

        if (dataToSend == null) {
            throw new ArgumentNullException(nameof(dataToSend), "Unable to send a `null` byte buffer !");
        }

        // We do not use a FileStream in here to prevent segmented messages being put in the mailslot.
        if (!MailslotBindings.WriteFile(MailslotHandle, dataToSend, (uint) dataToSend.Length, out uint _, IntPtr.Zero)) {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }*/

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
    /// <exception cref="InvalidOperationException">
    ///     Thrown when used on an async client.
    /// </exception>
    /// <exception cref="Win32Exception">
    ///     If there was an error while writing to the mailslot.
    /// </exception>
    /// <remarks>
    ///     This function will not handles cases where the server allows a smaller
    ///      message than what you're sending.<br/>
    ///     You should implement support for this behavious on your own if needed.
    /// </remarks>
    /// <remarks>
    ///     This function doens't handle async <see cref="MailslotClient"/> since
    ///      we're not managing complex structures for Win32 API calls yet.
    /// </remarks>
    /*public void SendSync(string textToSend, Encoding? encoding) {
        if (IsAsync) {
            throw new InvalidOperationException("Use Write/WriteAsync directly on this instance for async clients.");
        }

        if (textToSend == null) {
            throw new ArgumentNullException(nameof(textToSend), "Unable to send a `null` bit of text !");
        }

        if (encoding == null) {
            encoding = Encoding.Default;
        }

        SendSync(encoding.GetBytes(textToSend));
    }*/

    /// <summary>
    ///     Sends a given bit of text to the mailslot server.
    /// </summary>
    /// <param name="textToSend">
    ///     The bit of text to send.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the data to send is <c>null</c>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when used on an async client.
    /// </exception>
    /// <exception cref="Win32Exception">
    ///     If there was an error while writing to the mailslot.
    /// </exception>
    /// <remarks>
    ///     This function will not handles cases where the server allows a smaller
    ///      message than what you're sending.<br/>
    ///     You should implement support for this behavious on your own if needed.
    /// </remarks>
    /// <remarks>
    ///     This function doens't handle async <see cref="MailslotClient"/> since
    ///      we're not managing complex structures for Win32 API calls yet.
    /// </remarks>
    /*public void SendSync(string textToSend) {
        SendSync(textToSend, null);
    }*/


    #region Class methods

    /// <summary>
    ///     Validates the given UNC path, optionally checks that it already exists, and
    ///      opens the underlying mailslot client handle for it.
    /// </summary>
    /// <remarks>
    ///     Exists solely so the handle can be produced as an argument to this class'
    ///      <c>base(...)</c> (i.e. <see cref="FileStream"/>) constructor call, since that
    ///      must run before any instance code of this class does.
    /// </remarks>
    /// <exception cref="ArgumentException">
    ///     Thrown if the given UNC path is invalid.
    /// </exception>
    /// <exception cref="IOException">
    ///     Thrown if <paramref name="mustExist"/> is <c>true</c> and the server doesn't
    ///      exist or can't be connected to.
    /// </exception>
    /// <exception cref="Win32Exception">
    ///     Thrown if we got an invalid <see cref="SafeFileHandle"/> while opening
    ///      the mailslot in the Win32 APIs.
    /// </exception>
    private static SafeFileHandle CreateMailslotClientHandle(string fullUncPath, bool isAsync, bool mustExist) {
        if (!PathIsUNC(fullUncPath)) {
            throw new ArgumentException("Invalid combination of UNC host and path values !");
        }

        if (mustExist) {
            if (!File.Exists(fullUncPath)) {
                throw new IOException($"The resource at '{fullUncPath}' doesn't exist !");
            }
        }

        SafeFileHandle handle = CreateFile(
            fullUncPath,
            FileAccess.Write,    // Clients can only write
            FileShare.ReadWrite, // Shared with the server
            IntPtr.Zero,         // Ignored for mailslots
            FileMode.Open,       // Same as `OPEN_EXISTING`
            isAsync ? (FileAttributes) FILE_FLAG_OVERLAPPED : FILE_FLAG_NONE,
            IntPtr.Zero          // Ignored for mailslots
        );
        if (handle.IsInvalid) {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        return handle;
    }

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
        return new MailslotClient(fullUncPath, isAsync, mustExist, bufferSize);
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
        return new MailslotClient(host, mailslotPath, isAsync, mustExist, bufferSize);
    }

    #endregion

}
