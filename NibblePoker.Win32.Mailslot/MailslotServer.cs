using Microsoft.Win32.SafeHandles;
using NibblePoker.Win32.Mailslot.Exceptions;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace NibblePoker.Win32.Mailslot;

public class MailslotServer : Mailslot {

    public string FullPath {
        get;
        private set;
    }

    private readonly uint _maxMessageSize;
    public override uint MaxMessageSize {
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
        FullPath = $"\\\\.\\mailslot\\{path}";
        if (!PathIsUNC(FullPath)) {
            throw new InvalidUncPathException(".", "mailslot\\" + path);
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


    #region PInvoke

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern SafeFileHandle CreateMailslot(
        [In] string lpName,
        [In] uint nMaxMessageSize,
        [In] uint lReadTimeout,
        [In, Optional] IntPtr lpSecurityAttributes
    );

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.None, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMailslotInfo(
        [In] SafeHandle hMailslot,
        [Out, Optional] out uint? lpMaxMessageSize,
        [Out, Optional] out uint? lpNextSize,
        [Out, Optional] out uint? lpMessageCount,
        [Out, Optional] out uint? lpReadTimeout
    );

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.None, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetMailslotInfo(
      [In] SafeHandle hMailslot,
      [In] uint lReadTimeout
    );

    #endregion
}
