using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

using static NibblePoker.Win32.Mailslot.MailslotBindings;
using static NibblePoker.Win32.Mailslot.MailslotUtils;

namespace NibblePoker.Win32.Mailslot;

/// <summary>
/// Represents a mailslot server and provides all the utilities related to them.
/// </summary>
public class MailslotServer {
    /// <summary>
    /// There is no next message.
    /// </summary>
    public const uint MAILSLOT_NO_MESSAGE = MailslotConstants.MAILSLOT_NO_MESSAGE;

    /// <summary>
    /// Waits forever for a message.
    /// </summary>
    public const uint MAILSLOT_WAIT_FOREVER = MailslotConstants.MAILSLOT_WAIT_FOREVER;

    /// <summary>
    /// UNC path to which the client is connected.<br/>
    /// Format: <c>\\{domain}\mailslot\{path}</c>
    /// </summary>
    public string FullPath {
        get;
        private set;
    }

    private readonly uint _maxMessageSize;
    public uint MaxMessageSize {
        get => _maxMessageSize;
    }

    private uint _readTimeoutMs;
    public uint ReadTimeoutMs {
        get => _readTimeoutMs;
        set {
            if (SetMailslotInfo(MailslotHandle, value)) {
                _readTimeoutMs = value;
            } else {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }

    /// <summary>
    /// Represents the number of messages queued in the mailslot.
    /// </summary>
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
    /// Represents the size of the next message in the mailslot queue.
    /// </summary>
    public uint NextMessageSize {
        get {
            if (GetMailslotInfo(MailslotHandle, out uint dwReturnValue, out _, out _, out _)) {
                return dwReturnValue;
            } else {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }

    internal SafeFileHandle MailslotHandle;

    /// <summary>
    ///     Creates a mailslot server on the given UNC path.
    /// </summary>
    /// <param name="fullUncPath">
    ///     The UNC path at which the mailslot should be created.<br/>
    ///     Format: <c>\\{domain}\mailslot\{path}</c>
    /// </param>
    /// <param name="maxMessageSize"></param>
    /// <param name="readTimeoutMs"></param>
    /// <exception cref="ArgumentException">[Includes InvalidUncPathException]</exception>
    /// <exception cref="Win32Exception">
    ///     ??? <br/>
    ///     <see href="https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes"/>
    /// </exception>
    public MailslotServer(string fullUncPath, uint maxMessageSize, uint readTimeoutMs) {
        if (!PathIsUNC(fullUncPath)) {
            throw new ArgumentException($"The UNC path `{fullUncPath}` is invalid !", nameof(fullUncPath));
        }

        _maxMessageSize = maxMessageSize;

        FullPath = fullUncPath;

        MailslotHandle = CreateMailslot(fullUncPath, MaxMessageSize, readTimeoutMs, IntPtr.Zero);
        if (MailslotHandle.IsInvalid) {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    /// <summary>
    ///     Creates a mailslot server on the given UNC domain at the given mailslot path.
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
    /// <exception cref="ArgumentException">[Includes InvalidUncPathException]</exception>
    /// <exception cref="Win32Exception">
    ///     ??? <br/>
    ///     <see href="https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes"/>
    /// </exception>
    public MailslotServer(string? uncDomain, string mailslotPath, uint maxMessageSize, uint readTimeoutMs) {
        if (!IsValidUNCPath(mailslotPath)) {
            throw new ArgumentException($"Invalid mailslot path value ! ({mailslotPath})", nameof(mailslotPath));
        }

        if(uncDomain == null) {
            uncDomain = ".";
        }

        FullPath = $"\\\\{uncDomain}\\mailslot\\{mailslotPath}";
        if (!PathIsUNC(FullPath)) {
            throw new ArgumentException("Invalid combination of UNC domain and mailslot path values !");
        }

        _maxMessageSize = maxMessageSize;

        MailslotHandle = CreateMailslot(FullPath, MaxMessageSize, readTimeoutMs, IntPtr.Zero);
        if (MailslotHandle.IsInvalid) {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
    }

    public MailslotServer SetReadTimeoutMs(uint readTimeoutMs) {
        ReadTimeoutMs = readTimeoutMs;
        return this;
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
    ///     The returned FileStream doesn't own the <see cref="SafeFileHandle"/>, the <see cref="MailslotServer"/> instance does.
    /// </remarks>
    public FileStream GetFileStream(int bufferSize = 4096) {
        //var a = new FileStream(MailslotHandle, FileAccess.Read, bufferSize, true, ownsHandle: false);
        return new FileStream(MailslotHandle, FileAccess.Read, bufferSize, true);
    }

    /// <summary>
    ///     Creates a mailslot server and returns its <see cref="FileStream"/> directly.
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
        return new MailslotServer(fullUncPath, maxMessageSize, readTimeoutMs).GetFileStream(bufferSize);
    }

    /// <summary>
    ///     Creates a mailslot server and returns its <see cref="FileStream"/> directly.
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
        return new MailslotServer(uncDomain, mailslotPath, maxMessageSize, readTimeoutMs).GetFileStream(bufferSize);
    }

    /// <summary>
    ///     Checks if a mailslot exists at a given UNC path.
    /// </summary>
    /// <param name="uncPath"></param>
    /// <returns></returns>
    public static bool ExistsAt(string uncPath) {
        return File.Exists(uncPath);
    }

    /// <summary>
    ///     Checks if a mailslot exists in a given UNC domain and at a given mailslot path.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool ExistsAt(string host, string path) {
        return ExistsAt($"\\\\{host}\\mailslot\\{path}");
    }
}
