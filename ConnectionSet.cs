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
using System.Runtime.InteropServices;
using System.Security;
using System.Xml;

namespace VolatileDriveConnector
{
  /// <summary>
  /// 
  /// </summary>
  /// <seealso cref="System.IEquatable&lt;VolatileDriveConnector.ConnectionSet&gt;" />
  /// <seealso href="http://www.blackwasp.co.uk/mapdriveletter.aspx"/>
  public record ConnectionSet
  {
    public int TimeOut_Minutes { get; } = 10;
    public IReadOnlyList<DrivePathPair> Connections { get; }

    public void SaveXml(XmlWriter tw)
    {
      tw.WriteElementString("TimeoutMinutes", XmlConvert.ToString(TimeOut_Minutes));
      tw.WriteStartElement("Connections");
      tw.WriteAttributeString("Count", XmlConvert.ToString(Connections.Count));
      {
        foreach (var conn in Connections)
        {
          tw.WriteStartElement("Connection");
          {
            tw.WriteElementString("Drive", conn.DriveLetter);
            tw.WriteElementString("Path", conn.Path);
          }
          tw.WriteEndElement(); // Connection
        }
      }
      tw.WriteEndElement(); // Connections
    }

    public static ConnectionSet ReadXml(XmlReader tr)
    {
      var timeout = tr.ReadElementContentAsInt("TimeoutMinutes", string.Empty);

      var count = XmlConvert.ToInt32(tr.GetAttribute("Count"));
      tr.ReadStartElement("Connections");
      var conn = new List<EditableDrivePathPair>(count);
      for (int i = 0; i < count; ++i)
      {
        tr.ReadStartElement("Connection");
        {
          var drive = tr.ReadElementContentAsString("Drive", string.Empty);
          var path = tr.ReadElementContentAsString("Path", string.Empty);
          conn.Add(new EditableDrivePathPair { DriveLetter = drive, PathName = path });
        }
        tr.ReadEndElement(); // "Connection");
      }
      if (count > 0)
      {
        tr.ReadEndElement(); // Connections
      }

      return new ConnectionSet(conn, timeout);
    }

    public ConnectionSet(IEnumerable<EditableDrivePathPair> connections, int timeoutMinutes)
    {
      TimeOut_Minutes = timeoutMinutes;

      var myconnections = new List<DrivePathPair>();

      foreach (var conn in connections)
      {
        if (!string.IsNullOrEmpty(conn.DriveLetter) && !string.IsNullOrEmpty(conn.PathName))
        {
          myconnections.Add(new DrivePathPair(conn.DriveLetter, conn.PathName));
        }
      }

      Connections = myconnections.AsReadOnly();
    }

#if UseCsWin
        public unsafe List<string> CreateConnections(string username, object password)
        {
            var errors = new List<string>();
            foreach (var conn in Connections)
            {
                var substitution = new EnvironmentSubstitution(conn.Path, username);
                if (!string.IsNullOrEmpty(substitution.Error))
                {
                    errors.Add(substitution.Error);
                    continue;
                }
                var res = new Windows.Win32.NetworkManagement.WNet.NETRESOURCEW();

                res.dwScope = Windows.Win32.NetworkManagement.WNet.NET_RESOURCE_SCOPE.RESOURCE_CONNECTED;
                res.dwType = Windows.Win32.NetworkManagement.WNet.NET_RESOURCE_TYPE.RESOURCETYPE_DISK;

                fixed (char* lpDriveLetter = conn.DriveLetter)
                {
                    fixed (char* lpPath = substitution.Result)
                    {
                        res.lpLocalName = lpDriveLetter;
                        res.lpRemoteName = lpPath;
                        res.lpProvider = null;
                        var result = Windows.Win32.PInvoke.WNetAddConnection3W(Windows.Win32.Foundation.HWND.Null, &res, (string)password, username, 0);
                        if (result != 0)
                        {
                            errors.Add($"{DateTime.Now:HH:mm:ss} Error #{result} connecting {substitution.Result}");
                        }
                    }
                }
            }
            return errors;
        }

        public unsafe void DisposeConnections()
        {

            foreach (var conn in Connections)
            {
                fixed (char* lpDriveLetter = conn.DriveLetter)
                {
                    fixed (char* lpPath = conn.Path)
                    {
                        var result = Windows.Win32.PInvoke.WNetCancelConnection2W(lpDriveLetter, 0, true);
                    }
                }
            }
        }
#else

    public unsafe List<string> CreateConnections(string username, object password)
    {
      var errors = new List<string>();
      foreach (var conn in Connections)
      {
        var substitution = new EnvironmentSubstitution(conn.Path, username);
        if (!string.IsNullOrEmpty(substitution.Error))
        {
          errors.Add(substitution.Error);
          continue;
        }

        var res = new NativeMethods.NETRESOURCEW();

        res.dwScope = NativeMethods.RESOURCE_CONNECTED;
        res.dwType = NativeMethods.RESOURCETYPE_DISK;
        res.dwDisplayType = 0;
        res.dwUsage = 0;


        res.lpLocalName = Marshal.StringToBSTR(conn.DriveLetter);
        res.lpRemoteName = Marshal.StringToBSTR(substitution.Result);
        res.lpProvider = IntPtr.Zero;
        res.lpComment = IntPtr.Zero;


        IntPtr lpPassword, lpUserName;
        var result = NativeMethods.WNetAddConnection3W(IntPtr.Zero, new IntPtr(&res), lpPassword = Marshal.SecureStringToBSTR((SecureString)password), lpUserName = Marshal.StringToBSTR(username), 0);
        Marshal.FreeBSTR(lpPassword);
        Marshal.FreeBSTR(lpUserName);
        Marshal.FreeBSTR(res.lpLocalName);
        Marshal.FreeBSTR(res.lpRemoteName);
        if (result != 0)
        {
          errors.Add($"{DateTime.Now:HH:mm:ss} Error #{result} connecting {conn.DriveLetter} to {substitution.Result}");
        }
      }
      return errors;
    }

    public List<string> DisposeConnections()
    {
      var errors = new List<string>();

      foreach (var conn in Connections)
      {
        IntPtr lpDriveLetter;
        var result = NativeMethods.WNetCancelConnection2W(lpDriveLetter = Marshal.StringToBSTR(conn.DriveLetter), 0, true);
        Marshal.FreeBSTR(lpDriveLetter);
        if (result != 0)
        {
          errors.Add($"{DateTime.Now:HH:mm:ss} Error #{result} disconnecting {conn.Path}");
        }
      }
      return errors;
    }

#endif




  }
}
