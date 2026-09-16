using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

using static NibblePoker.Win32.Mailslot.MailslotBindings;

namespace NibblePoker.Win32.Mailslot;

/// <summary>
/// Represents a mailslot server and provides all the utilities related to them.
/// </summary>
/// <remarks>
///     This class <b>is</b> the <see cref="FileStream"/> you read from: there's no separate
///      handle to own, duplicate or hand out. Just pass the instance itself around to
///      wherever it needs to be read from.
/// </remarks>
public class MailslotServer : FileStream {

    /// <summary>
    ///     There is no next message.
    /// </summary>
    /// <remarks>
    ///     The value is <c>0xFFFFFFFF</c> instead of <c>-1</c> due to
    ///     the <c>((DWORD) -1)</c> typecast that is not supported in C#.
    /// </remarks>
    public const uint MAILSLOT_NO_MESSAGE = MailslotConstants.MAILSLOT_NO_MESSAGE;

    /// <summary>
    ///     Waits forever for a message.
    /// </summary>
    /// <remarks>
    ///     The value is <c>0xFFFFFFFF</c> instead of <c>-1</c> due to
    ///     the <c>((DWORD) -1)</c> typecast that is not supported in C#.
    /// </remarks>
    public const uint MAILSLOT_WAIT_FOREVER = MailslotConstants.MAILSLOT_WAIT_FOREVER;

    public const uint MAILSLOT_ANY_MESSAGE_SIZE = 0;


    #region Properties

    /// <summary>
    ///     UNC path to which the client is connected.<br/>
    ///     Format: <c>\\{domain}\mailslot\{path}</c>
    /// </summary>
    public string FullPath {
        get;
        private set;
    }

    /// <summary>
    ///     ???
    /// </summary>
    /// <remarks>
    ///     You should use <see cref="MailslotServer.GetInfo"/> if you're reading more
    ///      than 1 property in a given work unit.
    /// </remarks>
    public uint MaxMessageSize {
        get {
            if (GetMailslotInfo(MailslotHandle, out uint dwReturnValue, out _, out _, out _)) {
                return dwReturnValue;
            } else {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }

    /// <summary>
    ///     ???
    /// </summary>
    /// <remarks>
    ///     You should use <see cref="MailslotServer.GetInfo"/> if you're reading more
    ///      than 1 property in a given work unit.
    /// </remarks>
    public uint ReadTimeoutMs {
        get {
            if (GetMailslotInfo(MailslotHandle, out _, out _, out _, out uint dwReturnValue)) {
                return dwReturnValue;
            } else {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
        set {
            if (!SetMailslotInfo(MailslotHandle, value)) {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }

    /// <summary>
    ///     Represents the number of messages queued in the mailslot.
    /// </summary>
    /// <remarks>
    ///     You should use <see cref="MailslotServer.GetInfo"/> if you're reading more
    ///      than 1 property in a given work unit.
    /// </remarks>
    public uint MessageCount {
        get {
            if (GetMailslotInfo(MailslotHandle, out _, out _, out uint dwReturnValue, out _)) {
                return dwReturnValue;
            } else {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }

    /// <summary>
    ///     Represents the size of the next message in the mailslot queue.
    /// </summary>
    /// <remarks>
    ///     You should use <see cref="MailslotServer.GetInfo"/> if you're reading more
    ///      than 1 property in a given work unit.
    /// </remarks>
    public uint NextMessageSize {
        get {
            if (GetMailslotInfo(MailslotHandle, out _, out uint dwReturnValue, out _, out _)) {
                return dwReturnValue;
            } else {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }

    #endregion


    // Not set to private to enable some Win32 API based tests.
    // `SafeFileHandle` is inherited from `FileStream`: this instance's handle *is* the mailslot's handle.
    internal SafeFileHandle MailslotHandle {
        get => SafeFileHandle;
    }


    #region Constructors

    /// <summary>
    ///     Creates a mailslot server on the given UNC path.
    /// </summary>
    /// <param name="fullUncPath">
    ///     The UNC path at which the mailslot should be created.<br/>
    ///     Format: <c>\\{domain}\mailslot\{path}</c>
    /// </param>
    /// <param name="maxMessageSize">
    ///     Specifies the maximum message size in bytes that the server can receive.<br/>
    ///     If set to <c>0</c>, any size will be accepted.<br/>
    ///     If a message is larger than this limit, it will be ???.
    /// </param>
    /// <param name="readTimeoutMs"></param>
    /// <param name="bufferSize">
    ///     A positive <see cref="Int32"/> value greater than 0 indicating the buffer size.<br/>
    ///     The default buffer size is 4096.
    /// </param>
    /// <exception cref="ArgumentException">[Includes InvalidUncPathException]</exception>
    /// <exception cref="Win32Exception">
    ///     ??? <br/>
    ///     <see href="https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes"/>
    /// </exception>
    /// <remarks>
    ///     This instance is itself the <see cref="FileStream"/> backed by the mailslot's
    ///      handle: it owns that handle for its whole lifetime, and disposing it closes it.
    ///      Pass the instance around directly wherever it needs to be read from.
    /// </remarks>
    public MailslotServer(string fullUncPath, uint maxMessageSize, uint readTimeoutMs, int bufferSize = 4096) :
        base(CreateMailslotHandle(fullUncPath, maxMessageSize, readTimeoutMs), FileAccess.Read, bufferSize, isAsync: true) {
        FullPath = fullUncPath;
    }

    /// <summary>
    ///     Creates a mailslot server on the given UNC domain at the given mailslot path.
    /// </summary>
    /// <param name="host">
    ///     UNC host in which the mailslot should be created.
    ///     Leave as <c>null</c> to use <c>.</c>
    /// </param>
    /// <param name="mailslotPath">
    ///     The mailslot's path at which it should be available in the previously given domain.<br/>
    ///     Shouldn't contain the <c>mailslot\</c> part.
    /// </param>
    /// <param name="maxMessageSize">
    ///     Specifies the maximum message size in bytes that the server can receive.<br/>
    ///     If set to <c>0</c>, any size will be accepted.<br/>
    ///     If a message is larger than this limit, it will be ???.
    /// </param>
    /// <param name="readTimeoutMs"></param>
    /// <param name="bufferSize">
    ///     A positive <see cref="Int32"/> value greater than 0 indicating the buffer size.<br/>
    ///     The default buffer size is 4096.
    /// </param>
    /// <exception cref="ArgumentException">[Includes InvalidUncPathException]</exception>
    /// <exception cref="Win32Exception">
    ///     ??? <br/>
    ///     <see href="https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes"/>
    /// </exception>
    /// <remarks>
    ///     This instance is itself the <see cref="FileStream"/> backed by the mailslot's
    ///      handle: it owns that handle for its whole lifetime, and disposing it closes it.
    ///      Pass the instance around directly wherever it needs to be read from.
    /// </remarks>
    public MailslotServer(string? host, string mailslotPath, uint maxMessageSize, uint readTimeoutMs, int bufferSize = 4096) :
        this(
            MailslotUtils.ComposeMailslotUncPath(host != null ? host : ".", mailslotPath, false),
            maxMessageSize, readTimeoutMs, bufferSize
        ) { }

    #endregion


    public MailslotServer SetReadTimeoutMs(uint readTimeoutMs) {
        ReadTimeoutMs = readTimeoutMs;
        return this;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="maxMessageSize"></param>
    /// <param name="nextSize"></param>
    /// <param name="messageCount"></param>
    /// <param name="readTimeout"></param>
    /// <returns>
    ///     <c>true</c> if we could get info, <c>false</c> otherwise.
    /// </returns>
    public bool GetInfo(out uint maxMessageSize, out uint nextSize, out uint messageCount, out uint readTimeout) {
        return MailslotBindings.GetMailslotInfo(MailslotHandle, out maxMessageSize, out nextSize, out messageCount, out readTimeout);
    }


    #region Class methods

    /// <summary>
    ///     Validates the given UNC path and creates the underlying mailslot handle for it.
    /// </summary>
    /// <remarks>
    ///     Exists solely so the handle can be produced as an argument to this class'
    ///      <c>base(...)</c> (i.e. <see cref="FileStream"/>) constructor call, since that
    ///      must run before any instance code of this class does.
    /// </remarks>
    /// <exception cref="ArgumentException">[Includes InvalidUncPathException]</exception>
    /// <exception cref="Win32Exception">
    ///     ??? <br/>
    ///     <see href="https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes"/>
    /// </exception>
    private static SafeFileHandle CreateMailslotHandle(string fullUncPath, uint maxMessageSize, uint readTimeoutMs) {
        if (!PathIsUNC(fullUncPath)) {
            throw new ArgumentException($"The UNC path `{fullUncPath}` is invalid !", nameof(fullUncPath));
        }

        SafeFileHandle handle = CreateMailslot(fullUncPath, maxMessageSize, readTimeoutMs, IntPtr.Zero);
        if (handle.IsInvalid) {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        return handle;
    }

    /// <summary>
    ///     Creates a mailslot server and returns it as a <see cref="FileStream"/> directly.
    /// </summary>
    /// <param name="fullUncPath"></param>
    /// <param name="maxMessageSize"></param>
    /// <param name="readTimeoutMs"></param>
    /// <param name="bufferSize">
    ///     A positive <see cref="Int32"/> value greater than 0 indicating the buffer size.<br/>
    ///     The default buffer size is 4096.
    /// </param>
    /// <returns></returns>
    public static FileStream CreateAsFileStream(string fullUncPath, uint maxMessageSize, uint readTimeoutMs, int bufferSize = 4096) {
        return new MailslotServer(fullUncPath, maxMessageSize, readTimeoutMs, bufferSize);
    }

    /// <summary>
    ///     Creates a mailslot server and returns it as a <see cref="FileStream"/> directly.
    /// </summary>
    /// <param name="uncDomain">
    ///     UNC domain in which the mailslot should be created.
    ///     Leave as <c>null</c> to use <c>.</c>
    /// </param>
    /// <param name="mailslotPath">
    ///     The mailslot's path at which it should be available in the previously given domain.<br/>
    ///     Shouldn't contain the <c>mailslot\</c> part.
    /// </param>
    /// <param name="maxMessageSize"></param>
    /// <param name="readTimeoutMs"></param>
    /// <param name="bufferSize">
    ///     A positive <see cref="Int32"/> value greater than 0 indicating the buffer size.<br/>
    ///     The default buffer size is 4096.
    /// </param>
    /// <returns></returns>
    public static FileStream CreateAsFileStream(string? uncDomain, string mailslotPath, uint maxMessageSize, uint readTimeoutMs, int bufferSize = 4096) {
        return new MailslotServer(uncDomain, mailslotPath, maxMessageSize, readTimeoutMs, bufferSize);
    }

    /// <summary>
    ///     Checks if a mailslot exists at a given UNC path.
    /// </summary>
    /// <param name="uncPath"></param>
    /// <returns>
    ///     <c>true</c> if it exists, <c>false</c> otherwise.
    /// </returns>
    public static bool ExistsAt(string uncPath) {
        return File.Exists(uncPath);
    }

    /// <summary>
    ///     Checks if a mailslot exists on a given UNC host and at a given mailslot path.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="path"></param>
    /// <returns>
    ///     <c>true</c> if it exists, <c>false</c> otherwise.
    /// </returns>
    public static bool ExistsAt(string host, string path) {
        return ExistsAt($"\\\\{host}\\mailslot\\{path}");
    }

    #endregion

}
