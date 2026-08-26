namespace NibblePoker.Win32.Mailslot;

/// <summary>
/// Internal static class that contains all constants used internally or exposed in specific situations.
/// </summary>
internal static class MailslotConstants {
    internal const int FILE_FLAG_NONE = 0;

    internal const int FILE_FLAG_OVERLAPPED = 0x40000000;

    internal const int FILE_FLAG_NO_BUFFERING = 0x20000000;

    /// <remarks>
    ///     The value is <c>0xFFFFFFFF</c> instead of <c>-1</c> due to
    ///     the <c>((DWORD) -1)</c> typecast that is not supported in C#.
    /// </remarks>
    internal const uint MAILSLOT_NO_MESSAGE = 0xFFFFFFFF;

    /// <remarks>
    ///     The value is <c>0xFFFFFFFF</c> instead of <c>-1</c> due to
    ///     the <c>((DWORD) -1)</c> typecast that is not supported in C#.
    /// </remarks>
    internal const uint MAILSLOT_WAIT_FOREVER = 0xFFFFFFFF;
}
