// License unknown, see http://www.bryancook.net/2008/10/detect-wpf-lock-workstation-session.html

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace VolatileDriveConnector
{
  /// <summary>
  /// Notifies when a PC is locked or unlocked.
  /// </summary>
  /// <seealso href="http://www.bryancook.net/2008/10/detect-wpf-lock-workstation-session.html" />
  public class SessionNotificationUtil : IDisposable
  {
    // from wtsapi32.h
    private const int NotifyForThisSession = 0;

    // from winuser.h
    private const int SessionChangeMessage = 0x02B1;
    private const int SessionLockParam = 0x7;
    private const int SessionUnlockParam = 0x8;

    [DllImport("wtsapi32.dll")]
    private static extern bool WTSRegisterSessionNotification(IntPtr hWnd, int dwFlags);

    [DllImport("wtsapi32.dll")]
    private static extern bool WTSUnRegisterSessionNotification(IntPtr hWnd);

    // flag to indicate if we've registered for notifications or not
    private bool registered = false;
    private WindowInteropHelper interopHelper;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="window"></param>
    public SessionNotificationUtil(Window window)
    {
      interopHelper = new WindowInteropHelper(window);
      window.Loaded += new RoutedEventHandler(window_Loaded);
    }

    // deferred initialization logic
    private void window_Loaded(object sender, RoutedEventArgs e)
    {
      HwndSource source = HwndSource.FromHwnd(interopHelper.Handle);
      source.AddHook(new HwndSourceHook(WndProc));
      EnableRaisingEvents = true;
    }

    protected bool EnableRaisingEvents
    {
      get { return registered; }
      set
      {
        // WtsRegisterSessionNotification requires Windows XP or higher
        bool haveXp = Environment.OSVersion.Platform == PlatformID.Win32NT &&
             (Environment.OSVersion.Version.Major > 5 ||
             (Environment.OSVersion.Version.Major == 5 &&
             Environment.OSVersion.Version.Minor >= 1));

        if (!haveXp)
        {
          registered = false;
          return;
        }

        if (value == true && !registered)
        {
          WTSRegisterSessionNotification(interopHelper.Handle, NotifyForThisSession);
          registered = true;
        }
        else if (value == false && registered)
        {
          WTSUnRegisterSessionNotification(interopHelper.Handle);
          registered = false;
        }
      }
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
      if (msg == SessionChangeMessage)
      {
        if (wParam.ToInt32() == SessionLockParam)
        {
          OnSessionLock();
        }
        else if (wParam.ToInt32() == SessionUnlockParam)
        {
          OnSessionUnLock();
        }
      }
      return IntPtr.Zero;
    }

    private void OnSessionLock()
    {
      if (SessionChanged != null)
      {
        SessionChanged(this, new SessionNotificationEventArgs(SessionNotification.Lock));
      }
    }

    private void OnSessionUnLock()
    {
      if (SessionChanged != null)
      {
        SessionChanged(this, new SessionNotificationEventArgs(SessionNotification.Unlock));
      }
    }

    public event EventHandler<SessionNotificationEventArgs>? SessionChanged;

    #region IDisposable Members
    public void Dispose()
    {
      // unhook from wtsapi
      if (registered)
      {
        EnableRaisingEvents = false;
      }
    }
    #endregion
  }

  public class SessionNotificationEventArgs : EventArgs
  {
    public SessionNotificationEventArgs(SessionNotification notification)
    {
      _notification = notification;
      _timestamp = DateTime.Now;
    }

    public SessionNotification Notification
    {
      get { return _notification; }
    }

    public DateTime TimeStamp
    {
      get { return _timestamp; }
    }

    private SessionNotification _notification;
    private DateTime _timestamp;
  }

  public enum SessionNotification
  {
    Lock = 0,
    Unlock
  }

}
