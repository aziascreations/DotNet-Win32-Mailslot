using Microsoft.Win32.SafeHandles;
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
            if(SetMailslotInfo(MailslotHandle, value)) {
                _readTimeoutMs = value;
            } else {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }

    internal SafeFileHandle MailslotHandle;

    public MailslotServer(string path, uint maxMessageSize, uint readTimeoutMs) {
        FullPath = $"\\\\.\\mailslot\\{path}";
        _maxMessageSize = maxMessageSize ;

        MailslotHandle = CreateMailslot(FullPath, MaxMessageSize, readTimeoutMs, IntPtr.Zero);
        if(MailslotHandle.IsInvalid) {
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

    public static FileStream CreateFileStream(string path, uint maxMessageSize, uint readTimeoutMs, int bufferSize = 4096) {
        return new MailslotServer(path, maxMessageSize, readTimeoutMs).GetFileStream();
    }
}
