#region Copyright

/////////////////////////////////////////////////////////////////////////////////
// MIT License
// 
// Copyright (c) 2023 Dirk Lellinger
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
/////////////////////////////////////////////////////////////////////////////////

#endregion Copyright

using System;
using System.ComponentModel;
using System.Windows;

namespace VolatileDriveConnector
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
      this.DataContext = new MainController();
      SessionNotificationUtil util = new SessionNotificationUtil(this);
      util.SessionChanged += new EventHandler<SessionNotificationEventArgs>(EhSessionChanged);

    }

    /// <summary>
    /// Called when the user locks the screen. 
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="SessionNotificationEventArgs"/> instance containing the event data.</param>
    private void EhSessionChanged(object? sender, SessionNotificationEventArgs e)
    {
      System.Diagnostics.Debug.WriteLine($"Received {e.Notification} notification at {e.TimeStamp}");
      ((MainController)DataContext).CmdDisconnect.Execute(null);
    }

    private void EhConnect(object sender, RoutedEventArgs e)
    {
      MakeConnection();
    }

    private void MakeConnection()
    {

#if UseCsWin
            var pwd = _edPwd.Password;
#else
      var pwd = _edPwd.SecurePassword;
#endif
      _edPwd.Clear();
      ((MainController)DataContext).CmdConnect.Execute(pwd);
    }

    /// <summary>
    /// If the window is closing, then disconnect all drives.
    /// </summary>
    /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs" /> that contains the event data.</param>
    protected override void OnClosing(CancelEventArgs e)
    {
      ((MainController)DataContext).CmdDisconnect.Execute(null);
      base.OnClosing(e);
    }

    /// <summary>
    /// If the user presses ENTER in the password box, after he has entered a password, then connect to the drives.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
    private void EhKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.Enter && object.ReferenceEquals(e.OriginalSource, _edPwd) && _edPwd.SecurePassword.Length > 0)
      {
        MakeConnection();
      }
    }
  }
}
