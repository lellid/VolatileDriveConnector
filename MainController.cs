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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Security;
using System.Xml;
using VolatileDriveConnector.Gui;


namespace VolatileDriveConnector
{
  /// <summary>
  /// Controller for the main window.
  /// </summary>
  /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
  public class MainController : INotifyPropertyChanged
  {
    private System.Windows.Threading.DispatcherTimer _dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
    private DateTimeOffset? _connectionPeriodElapsed;



    private Dictionary<string, ConnectionSet> _userConnections = new Dictionary<string, ConnectionSet>();

    public DateTimeOffset? _scheduledTimeout { get; set; }

    #region Bindings

    public RelayCommand CmdAddConnection { get; }
    public RelayCommand CmdRemoveConnection { get; }

    public RelayCommand<object> CmdConnect { get; }

    public RelayCommand CmdDisconnect { get; }

    public RelayCommand CmdClearMessages { get; }


    private int _timeoutMinutes = 10;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public int TimeoutMinutes
    {
      get => _timeoutMinutes;
      set
      {
        if (!(_timeoutMinutes == value))
        {
          _timeoutMinutes = value;
          OnPropertyChanged(nameof(TimeoutMinutes));
        }
      }
    }

    private ObservableCollection<EditableDrivePathPair> _connections = new ObservableCollection<EditableDrivePathPair>();

    public ObservableCollection<EditableDrivePathPair> Connections
    {
      get => _connections;
    }


    private object? _selectedConnection;

    public object? SelectedConnection
    {
      get => _selectedConnection;
      set
      {
        if (!(_selectedConnection == value))
        {
          _selectedConnection = value;
          OnPropertyChanged(nameof(SelectedConnection));
        }
      }
    }

    private string _userName = string.Empty;

    public string UserName
    {
      get => _userName;
      set
      {
        if (!(_userName == value))
        {
          _userName = value;
          OnPropertyChanged(nameof(UserName));
          if (_userConnections.TryGetValue(value, out var connection))
          {
            TimeoutMinutes = connection.TimeOut_Minutes;
            Connections.Clear();
            foreach (var c in connection.Connections)
            {
              Connections.Add(new EditableDrivePathPair { DriveLetter = c.DriveLetter, PathName = c.Path });
            }
          }
        }
      }
    }

    public ObservableCollection<string> UserNames { get; } = new ObservableCollection<string>();

    public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();

    private ConnectionSet? _currentActiveConnection;

    private ConnectionSet? CurrentActiveConnection
    {
      get => _currentActiveConnection;
      set
      {
        if (!(_currentActiveConnection == value))
        {
          _currentActiveConnection = value;
          OnPropertyChanged(nameof(CurrentActiveConnection));
          OnPropertyChanged(nameof(IsConnectionActive));
          OnPropertyChanged(nameof(IsConnectionInactive));
        }
      }
    }
    public bool IsConnectionActive => CurrentActiveConnection is not null;
    public bool IsConnectionInactive => CurrentActiveConnection is null;

    private TimeSpan _countDown;

    public TimeSpan CountDown
    {
      get => _countDown;
      set
      {
        if (!(_countDown == value))
        {
          _countDown = value;
          OnPropertyChanged(nameof(CountDown));
        }
      }
    }


    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="MainController"/> class.
    /// </summary>
    public MainController()
    {
      CmdAddConnection = new RelayCommand(EhAddConnection);
      CmdRemoveConnection = new RelayCommand(EhRemoveConnection);
      CmdConnect = new RelayCommand<object>(EhConnect);
      CmdDisconnect = new RelayCommand(EhDisconnect);
      CmdClearMessages = new RelayCommand(EhClearMessages);
      LoadSettings();
    }

    /// <summary>
    /// Occurs every second, if the count down timer is active.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void EhTimerElapsed(object? sender, EventArgs e)
    {
      if (!_connectionPeriodElapsed.HasValue || DateTimeOffset.UtcNow >= _connectionPeriodElapsed.Value)
      {
        EhDisconnect();
      }
      else
      {
        CountDown = TimeSpan.FromSeconds(Math.Round((_connectionPeriodElapsed.Value - DateTimeOffset.UtcNow).TotalSeconds));
      }
    }

    /// <summary>
    /// Occurs when the user wants to disconnect all drives.
    /// </summary>
    private void EhDisconnect()
    {
      _dispatcherTimer.Stop();
      CountDown = TimeSpan.FromSeconds(0);
      var errors = CurrentActiveConnection?.DisposeConnections();
      CurrentActiveConnection = null;

      if (errors is not null && errors.Count > 0)
      {
        for (int i = errors.Count - 1; i >= 0; --i)
        {
          Messages.Insert(0, errors[i]);
        }
      }
    }

    /// <summary>
    /// Occurs when the user wants to connect all drives.
    /// </summary>
    /// <param name="password">The password (either a <see cref="SecureString"/>, or if using CsWin32, a plain string.</param>
    private void EhConnect(object password)
    {
      if (string.IsNullOrEmpty(UserName))
      {
        ShowNoUserNameDialog();
        return;
      }

      if (CurrentActiveConnection is not null)
      {
        ShowActiveConnectionError();
        return;
      }

      if ((password is string s && string.IsNullOrEmpty(s)) || (password is SecureString ss && ss.Length == 0))
      {
        var q = System.Windows.MessageBox.Show("You have not entered a password. Do you want to proceed with an empty password?", "Password empty!", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question, System.Windows.MessageBoxResult.No);
        if (q == System.Windows.MessageBoxResult.No)
        {
          return;
        }
      }

      var currentActiveConnection = new ConnectionSet(Connections, TimeoutMinutes);
      _userConnections[UserName] = currentActiveConnection;
      var errors = currentActiveConnection.CreateConnections(UserName, password);
      if (errors.Count > 0)
      {
        for (int i = errors.Count - 1; i >= 0; --i)
        {
          Messages.Insert(0, errors[i]);
        }
      }

      CurrentActiveConnection = currentActiveConnection;
      SaveSettings();

      _dispatcherTimer.Tick -= EhTimerElapsed;
      _dispatcherTimer.Tick += EhTimerElapsed;
      _dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
      _connectionPeriodElapsed = DateTimeOffset.UtcNow + TimeSpan.FromMinutes(TimeoutMinutes);
      _dispatcherTimer.Start();
    }



    /// <summary>
    /// Occurs when the user wants to remove one connection item.
    /// </summary>
    private void EhRemoveConnection()
    {
      for (int i = Connections.Count - 1; i >= 0; i--)
      {
        if (object.ReferenceEquals(SelectedConnection, Connections[i]))
        {
          Connections.RemoveAt(i);
          break;
        }
      }
    }

    /// <summary>
    /// Occurs when the user wants to add one connection item.
    /// </summary>
    private void EhAddConnection()
    {
      if (string.IsNullOrEmpty(UserName))
      {
        ShowNoUserNameDialog();
        return;
      }
      Connections.Add(new EditableDrivePathPair());
    }

    /// <summary>
    /// Occurs when the user wants to clear all error messages.
    /// </summary>
    private void EhClearMessages()
    {
      Messages.Clear();
    }

    private void ShowNoUserNameDialog()
    {
      System.Windows.MessageBox.Show("Please enter a user name", "User name required", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Exclamation);
    }

    private void ShowActiveConnectionError()
    {
      System.Windows.MessageBox.Show("Please disconnect before connecting anew", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
    }





    /// <summary>
    /// Gets the path to settings file.
    /// </summary>
    /// <returns>Path to the settings file.</returns>
    private string GetPathToSettingsFile()
    {
      var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      path = System.IO.Path.Combine(path, "VolatileDriveConnector");
      path = System.IO.Path.Combine(path, "Settings.xml");
      return path;
    }

    /// <summary>
    /// Saves the settings.
    /// </summary>
    private void SaveSettings()
    {
      var fileName = GetPathToSettingsFile();
      Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fileName));
      using var tw = XmlWriter.Create(fileName);

      tw.WriteStartDocument();
      {
        tw.WriteStartElement("Settings");
        {
          tw.WriteStartElement("Users");
          tw.WriteAttributeString("Count", XmlConvert.ToString(_userConnections.Count));
          {
            foreach (var user in _userConnections)
            {
              tw.WriteStartElement("User");
              {
                tw.WriteElementString("UserName", user.Key);
                tw.WriteStartElement("Connection");
                user.Value.SaveXml(tw);
                tw.WriteEndElement(); // Connection
              }
              tw.WriteEndElement(); // User
            }
          }
          tw.WriteEndElement(); // users

          tw.WriteElementString("LastUserName", UserName);
        }
        tw.WriteEndElement(); //Settings
      }
      tw.WriteEndDocument();
    }

    /// <summary>
    /// Loads the settings from the settings file.
    /// </summary>
    private void LoadSettings()
    {
      if (!System.IO.File.Exists(GetPathToSettingsFile()))
      {
        return;
      }

      var dict = new Dictionary<string, ConnectionSet>();
      string lastUserName;

      try
      {

        using var tr = XmlReader.Create(GetPathToSettingsFile());

        tr.MoveToContent();

        tr.ReadStartElement("Settings");
        {

          var count = XmlConvert.ToInt32(tr.GetAttribute("Count"));
          tr.ReadStartElement("Users");
          for (int i = 0; i < count; ++i)
          {
            tr.ReadStartElement("User");
            {
              var name = tr.ReadElementContentAsString("UserName", string.Empty);
              tr.ReadStartElement("Connection");
              {
                var conn = ConnectionSet.ReadXml(tr);
                dict[name] = conn;
              }
              tr.ReadEndElement(); // Connection

            }
            tr.ReadEndElement(); // user

          }
          if (count > 0)
          {
            tr.ReadEndElement(); // Users
          }

          lastUserName = tr.ReadElementContentAsString("LastUserName", string.Empty);
        }
        tr.ReadEndElement(); // Settings

        _userConnections = dict;

        var userNames = new List<string>(_userConnections.Keys);
        userNames.Sort();
        UserNames.Clear();
        foreach (var n in userNames)
        {
          UserNames.Add(n);
        }
        UserName = lastUserName;
      }
      catch (Exception ex)
      {
        Messages.Insert(0, $"{DateTime.Now:HH:mm:ss} Error loading settings: {ex.Message}");
      }
    }
  }
}
