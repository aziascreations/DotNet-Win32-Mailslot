using Microsoft.Win32.SafeHandles;
using System;
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



    #region Bindings

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern SafeFileHandle CreateMailslot(
        [In] string lpName,
        [In] uint nMaxMessageSize,
        [In] uint lReadTimeout,
        [In, Optional] IntPtr? lpSecurityAttributes
    );

    /// <summary>
    /// Retrieves information about the specified mailslot.
    /// </summary>
    /// <param name="hMailslot">
    ///     A handle to a mailslot.<br/>
    ///     The CreateMailslot function must create this handle.
    /// </param>
    /// <param name="lpMaxMessageSize">
    ///     The maximum message size, in bytes, allowed for this mailslot.<br/>
    ///     This value can be greater than or equal to the value specified in the cbMaxMsg
    ///      parameter of the CreateMailslot function that created the mailslot.<br/>
    ///     This parameter can be NULL.<br/>
    /// </param>
    /// <param name="lpNextSize">
    ///     The size of the next message, in bytes.<br/>
    ///     The following value has special meaning.
    /// </param>
    /// <param name="lpMessageCount"></param>
    /// <param name="lpReadTimeout"></param>
    /// <returns></returns>
    [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.None, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetMailslotInfo(
        [In] SafeHandle hMailslot,
        [Out, Optional] out uint? lpMaxMessageSize,
        [Out, Optional] out uint? lpNextSize,
        [Out, Optional] out uint? lpMessageCount,
        [Out, Optional] out uint? lpReadTimeout
    );

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.None, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool SetMailslotInfo(
      [In] SafeHandle hMailslot,
      [In] uint lReadTimeout
    );

    #endregion

}
