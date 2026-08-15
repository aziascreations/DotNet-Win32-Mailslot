using System.IO;
using System.Runtime.InteropServices;

namespace NibblePoker.Win32.Mailslot;

public abstract class Mailslot {

    #region Constants

    /// <summary>
    /// There is no next message.
    /// </summary>
    public const int MAILSLOT_NO_MESSAGE = -1;

    /// <summary>
    /// Waits forever for a message.
    /// </summary>
    public const int MAILSLOT_WAIT_FOREVER = -1;

    #endregion



    #region Properties

    public abstract uint MaxMessageSize {
        get;
    }


    /*
       [out, optional] LPDWORD lpMaxMessageSize,
  [out, optional] LPDWORD lpNextSize,
  [out, optional] LPDWORD lpMessageCount,
  [out, optional] LPDWORD lpReadTimeout
     */

    #endregion


    #region PInvoke

    [DllImport("Shlwapi.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = false)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool PathIsUNC(
        [In] string pszPath
    );

    #endregion
}
