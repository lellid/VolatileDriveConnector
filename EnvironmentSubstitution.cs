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
using System.Text;

namespace VolatileDriveConnector
{
  /// <summary>
  /// Enables substitution of tokens in the path by environment variables. The name of the environment variable must be included in < > chars.
  /// </summary>
  public class EnvironmentSubstitution
  {
    /// <summary>
    /// Gets the resulting path.
    /// </summary>
    public string Result { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the error. If the substitution was successful, the value is string.Empty.
    /// </summary>
    public string Error { get; private set; } = string.Empty;

    private string _userName;

    private StringBuilder _stb;

    private string _path;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentSubstitution"/> class.
    /// </summary>
    /// <param name="path">The UNC path.</param>
    /// <param name="userName">The user name. This is neccessary, because the token &lt;UserName&gt; (case sensitive!) is not replaced with the environment variable UserName, but with the user name which
    /// was given in the program to connect to the drives. If the environment variable 'UserName' is required, use another casing, e.g. '&lt;USERNAME&gt;'</param>
    /// <remarks>The result is given in property <see cref="Result"/>, but check for errors first by inspecting <see cref="Error"/>.</remarks>
    public EnvironmentSubstitution(string path, string userName)
    {
      _path = path;
      _userName = userName;
      _stb = new StringBuilder();

      int? startIndex = null;
      int? endIndex = 0;
      for (int i = 0; i < path.Length; ++i)
      {
        if (path[i] == '<')
        {
          if (!endIndex.HasValue)
          {
            Error = $"{DateTime.Now:HH:mm:ss} Expected a '>' char at pos.{i} in path {path}";
            return;
          }
          startIndex = i;
          // copy all from endIndex up to here
          _stb.Append(path.Substring(endIndex.Value, i - endIndex.Value));
          endIndex = null;
        }
        else if (path[i] == '>')
        {
          if (!startIndex.HasValue)
          {
            Error = $"{DateTime.Now:HH:mm:ss} Expected a '<' char before pos.{i} in path {path}";
            return;
          }
          endIndex = i + 1;
          Substitute(startIndex.Value, endIndex.Value);
          if (!string.IsNullOrEmpty(Error))
          {
            return;
          }
          startIndex = null;
        }
      }
      if (!endIndex.HasValue)
      {
        Error = $"{DateTime.Now:HH:mm:ss} Expected a '>' char at pos.{_path.Length - 1} in path {path}";
        return;
      }
      else if (endIndex < _path.Length)
      {
        _stb.Append(_path.Substring(endIndex.Value, path.Length - endIndex.Value));
      }

      Result = _stb.ToString();
    }

    private void Substitute(int startIndex, int endIndex)
    {
      var envName = _path.Substring(startIndex + 1, endIndex - startIndex - 2);
      if (envName == "UserName") // Attention: case sensitive! All other casings will use the environment variable instead
      {
        _stb.Append(_userName); // use the user name entered in the program instead of the environment variable
      }
      else
      {

        var substitute = Environment.GetEnvironmentVariable(envName);
        if (substitute is null)
        {
          Error = $"{DateTime.Now:HH:mm:ss} Unable to resolve '{envName}' in {_path}";
        }
        else
        {
          _stb.Append(substitute);
        }
      }
    }
  }
}
