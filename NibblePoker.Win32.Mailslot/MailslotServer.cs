using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

using static NibblePoker.Win32.Mailslot.MailslotBindings;
using static NibblePoker.Win32.Mailslot.MailslotUtils;

namespace NibblePoker.Win32.Mailslot;

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
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="maxMessageSize"></param>
    /// <param name="readTimeoutMs"></param>
    /// <exception cref="InvalidUncPathException"></exception>
    /// <exception cref="ArgumentException">[Includes InvalidUncPathException]</exception>
    /// <exception cref="Win32Exception">
    ///     ??? <br/>
    ///     <see href="https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes"/>
    /// </exception>
    public MailslotServer(string path, uint maxMessageSize, uint readTimeoutMs) {
        if (!IsValidUNCPath(path)) {
            throw new ArgumentException($"Invalid UNC path value ! ({path})", nameof(path));
        }

        FullPath = $"\\\\.\\mailslot\\{path}";
        if (!PathIsUNC(FullPath)) {
            throw new ArgumentException("Invalid combination of UNC host and path values !");
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

    public FileStream GetFileStream(int bufferSize = 4096) {
        return new FileStream(MailslotHandle, FileAccess.Read, bufferSize, true);
    }

    public static FileStream CreateAsFileStream(string path, uint maxMessageSize, uint readTimeoutMs, int bufferSize = 4096) {
        return new MailslotServer(path, maxMessageSize, readTimeoutMs).GetFileStream(bufferSize);
    }

    public static bool ExistsAt(string uncPath) {
        return File.Exists(uncPath);
    }

    public static bool ExistsAt(string host, string path) {
        return ExistsAt($"\\\\{host}\\{path}");
    }
}
