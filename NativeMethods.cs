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
using System.Runtime.InteropServices;

namespace VolatileDriveConnector
{
  public class NativeMethods
  {
    /* Uses Struct rather than class version of NETRESOURCE */
    [DllImport("mpr.dll")]
    public static extern int WNetAddConnection3W(IntPtr hWndOwner,
         IntPtr lpNetResource, IntPtr lpPassword,
        IntPtr lpUserName, int dwFlags);

    [DllImport("mpr.dll")]
    public static extern int WNetCancelConnection2W(IntPtr lpName, Int32 dwFlags, bool bForce);

    public const int RESOURCE_CONNECTED = 1;
    public const int RESOURCETYPE_DISK = 0x01;

    public struct NETRESOURCEW
    {
      public int dwScope;
      public int dwType;
      public int dwDisplayType;
      public int dwUsage;
      public IntPtr lpLocalName;
      public IntPtr lpRemoteName;
      public IntPtr lpComment;
      public IntPtr lpProvider;

    }
  }
}
