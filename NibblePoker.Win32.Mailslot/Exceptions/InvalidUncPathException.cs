using System;

namespace NibblePoker.Win32.Mailslot.Exceptions;

public class InvalidUncPathException : ArgumentException {
    public string Domain { get; }
    public string Path { get; }

    public InvalidUncPathException(string domain, string path) : base($"Invalid UNC path for domain '{domain}': '{path}' !") {
        Domain = domain;
        Path = path;
    }
}
