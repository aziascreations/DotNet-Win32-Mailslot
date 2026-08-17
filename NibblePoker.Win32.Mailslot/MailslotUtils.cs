using System.IO;
using System.Linq;

namespace NibblePoker.Win32.Mailslot;

internal static class MailslotUtils {
    internal static bool IsValidUNCHost(string host) {
        if (string.IsNullOrEmpty(host) || host.Length > 255) {
            return false;
        }

        if (host.Contains('\\') || host.Contains('/')) {
            return false;
        }

        if (host.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
            return false;
        }

        return true;
    }

    internal static bool IsValidUNCPath(string path) {
        if (string.IsNullOrEmpty(path)) {
            return false;
        }

        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0) {
            return false;
        }

        return true;
    }
}
