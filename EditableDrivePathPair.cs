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

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace VolatileDriveConnector
{
  /// <summary>
  /// Mutable entity to store a drive letter and the corresponding UNC path.
  /// </summary>
  public class EditableDrivePathPair : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private string _driveLetter = string.Empty;

    /// <summary>
    /// Gets or sets the drive letter.
    /// </summary>
    public string DriveLetter
    {
      get => _driveLetter;
      set
      {
        if (!(_driveLetter == value))
        {
          _driveLetter = value;
          OnPropertyChanged(nameof(DriveLetter));
        }
      }
    }

    private string _pathName = string.Empty;

    /// <summary>
    /// Gets or sets the UNC path.
    /// </summary>
    public string PathName
    {
      get => _pathName;
      set
      {
        if (!(_pathName == value))
        {
          _pathName = value;
          OnPropertyChanged(nameof(PathName));
        }
      }
    }

    /// <summary>
    /// Gets the list of drive letters (A-Z).
    /// </summary>
    public ObservableCollection<string> AvailableDriveLetters => _availableDriveLetters;

    public static ObservableCollection<string> _availableDriveLetters { get; }

    static EditableDrivePathPair()
    {
      _availableDriveLetters = new ObservableCollection<string>();
      for (char i = 'A'; i <= 'Z'; ++i)
      {
        _availableDriveLetters.Add(string.Empty + i + ":");
      }
    }

  }
}
