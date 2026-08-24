namespace NibblePoker.Win32.Mailslot;

/// <summary>
/// Internal static class that contains all constants used internally or exposed in specific situations.
/// </summary>
internal static class MailslotConstants {
    internal const int FILE_FLAG_NONE = 0;

    internal const int FILE_FLAG_OVERLAPPED = 0x40000000;

    internal const int FILE_FLAG_NO_BUFFERING = 0x20000000;

    /// <summary>
    /// Same as a <c>((DWORD) -1)</c> from the Win32 APIs.
    /// </summary>
    internal const uint MAILSLOT_NO_MESSAGE = 0xFFFFFFFF;

    /// <summary>
    /// Same as a <c>((DWORD) -1)</c> from the Win32 APIs.
    /// </summary>
    internal const uint MAILSLOT_WAIT_FOREVER = 0xFFFFFFFF;
}
