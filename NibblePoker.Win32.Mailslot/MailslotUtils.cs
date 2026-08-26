using System;
using System.IO;
using System.Linq;

namespace NibblePoker.Win32.Mailslot;

/// <summary>
///     Contains utility functions for UNC paths and its parts.
/// </summary>
public static class MailslotUtils {

    /// <summary>
    ///     Checks if the host part of a UNC path is valid.
    /// </summary>
    /// <param name="uncHostPart">UNC host part to check.</param>
    /// <returns><c>true</c> if it is valid, <c>false</c> otherwise.</returns>
    public static bool IsUncHostPartValid(string uncHostPart) {
        if (string.IsNullOrEmpty(uncHostPart) || uncHostPart.Length > 255) {
            return false;
        }

        if (uncHostPart.Contains('\\') || uncHostPart.Contains('/')) {
            return false;
        }

        if (uncHostPart.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Checks if the path part of a UNC path is valid.
    /// </summary>
    /// <param name="uncPathPart">UNC path part to check.</param>
    /// <returns><c>true</c> if it is valid, <c>false</c> otherwise.</returns>
    public static bool IsUncPathPartValid(string uncPathPart) {
        if (string.IsNullOrEmpty(uncPathPart)) {
            return false;
        }

        if (uncPathPart.IndexOfAny(Path.GetInvalidPathChars()) >= 0) {
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Checks if the given full UNC path is valid.
    /// </summary>
    /// <param name="uncFullPath">Full UNC path to check.</param>
    /// <returns><c>true</c> if it is valid, <c>false</c> otherwise.</returns>
    public static bool UsUncPathValid(string uncFullPath) {
        return MailslotBindings.PathIsUNC(uncFullPath);
    }

    /// <summary>
    ///     Attempts to compose a mailslot UNC path using the given host and path.<br/>
    ///     Format: <c>\\{host}\mailslot\{path}</c>
    /// </summary>
    /// <param name="uncHostPart"></param>
    /// <param name="uncPathPart"></param>
    /// <param name="checkCombinedPath">
    ///     Toggles a check done on the final UNC path.<br/>
    ///     Default: <c>true</c>, just to be extra safe.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">
    ///     If any given parameter is invalid.
    /// </exception>
    public static string ComposeMailslotUncPath(string uncHostPart, string uncPathPart, bool checkCombinedPath = true) {
        if (!IsUncHostPartValid(uncHostPart)) {
            throw new ArgumentException("Invalid UNC host.", nameof(uncHostPart));
        }

        if (!IsUncPathPartValid(uncPathPart)) {
            throw new ArgumentException("Invalid mailslot path.", nameof(uncPathPart));
        }

        string returnValue = $"\\\\{uncHostPart}\\mailslot\\{uncPathPart}";

        if (checkCombinedPath) {
            if (!UsUncPathValid(returnValue)) {
                throw new ArgumentException("Invalid combination of UNC host and path values !");
            }
        }

        return returnValue;
    }

    /// <summary>
    ///     Attempts to compose a mailslot UNC path using the given host and path.<br/>
    ///     Format: <c>\\{host}\mailslot\{path}</c>
    /// </summary>
    /// <param name="uncHostPart"></param>
    /// <param name="uncPathPart"></param>
    /// <param name="fullUncPath"></param>
    /// <param name="checkCombinedPath">
    ///     Toggles a check done on the final UNC path.<br/>
    ///     Default: <c>true</c>, just to be extra safe.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static bool TryComposeMailslotUncPath(string uncHostPart, string uncPathPart, out string fullUncPath, bool checkCombinedPath = true) {
        try {
            fullUncPath = ComposeMailslotUncPath(uncHostPart, uncPathPart, checkCombinedPath);
            return true;
        } catch(Exception) {
            fullUncPath = "";
            return false;
        }
    }
}
