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

namespace VolatileDriveConnector.Gui
{
  /// <summary>
  /// Implements an <see cref="System.Windows.Input.ICommand"/> by storing and executing actions for <see cref="System.Windows.Input.ICommand.Execute(object)"/>
  /// and <see cref="System.Windows.Input.ICommand.CanExecute(object)"/>. Note that you have to manually call <see cref="OnCanExecuteChanged"/> if <see cref="CanExecute(object)"/> will return a different value than before.
  /// </summary>
  /// <seealso cref="System.Windows.Input.ICommand" />
  public class RelayCommand : System.Windows.Input.ICommand
  {
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    #region Constructors

    public RelayCommand(Action execute)
     : this(execute, null)
    {
    }

    public RelayCommand(Action execute, Func<bool>? canExecute)
    {
      _execute = execute ?? throw new ArgumentNullException(nameof(execute));
      _canExecute = canExecute;
    }

    #endregion Constructors

    #region ICommand

    /// <summary>
    /// Occurs when changes occur that affect whether or not the command should execute.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// If called, will raise the <see cref="CanExecuteChanged"/> event. Call this function manually if <see cref="CanExecute(object)"/> will return a different value than before.
    /// </summary>
    public void OnCanExecuteChanged()
    {
      CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Used to connect to the Wpf CommandManager's RequerySuggested event that is fired if something in the Gui has changed.
    /// </summary>
    /// <param name="sender">The sender (unused).</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data (unused).</param>
    public void EhRequerySuggested(object? sender, EventArgs e)
    {
      OnCanExecuteChanged();
    }

    /// <summary>
    /// Defines the method that determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
    /// <returns>
    /// true if this command can be executed; otherwise, false.
    /// </returns>
    public bool CanExecute(object? parameter)
    {
      return _canExecute?.Invoke() ?? true;
    }

    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
    public void Execute(object? parameter)
    {
      _execute();
    }

    #endregion ICommand
  }

  /// <summary>
  /// Implements an <see cref="System.Windows.Input.ICommand"/> by storing and executing actions for <see cref="System.Windows.Input.ICommand.Execute(object)"/>
  /// and <see cref="System.Windows.Input.ICommand.CanExecute(object?)"/>. Note that you have to manually call <see cref="OnCanExecuteChanged"/> if <see cref="CanExecute(object)"/> will return a different value than before.
  /// </summary>
  /// <seealso cref="System.Windows.Input.ICommand" />
  public class RelayCommandWP : System.Windows.Input.ICommand
  {
    private readonly Action<object?> _execute;
    private readonly Func<bool>? _canExecute;

    #region Constructors

    public RelayCommandWP(Action<object> execute)
     : this(execute, null)
    {
    }

    public RelayCommandWP(Action<object> execute, Func<bool>? canExecute)
    {
      _execute = execute ?? throw new ArgumentNullException(nameof(execute));
      _canExecute = canExecute;
    }

    #endregion Constructors

    #region ICommand

    /// <summary>
    /// Occurs when changes occur that affect whether or not the command should execute.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// If called, will raise the <see cref="CanExecuteChanged"/> event. Call this function manually if <see cref="CanExecute(object)"/> will return a different value than before.
    /// </summary>
    public void OnCanExecuteChanged()
    {
      CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Used to connect to the Wpf CommandManager's RequerySuggested event that is fired if something in the Gui has changed.
    /// </summary>
    /// <param name="sender">The sender (unused).</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data (unused).</param>
    public void EhRequerySuggested(object? sender, EventArgs e)
    {
      OnCanExecuteChanged();
    }

    /// <summary>
    /// Defines the method that determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
    /// <returns>
    /// true if this command can be executed; otherwise, false.
    /// </returns>
    public bool CanExecute(object? parameter)
    {
      return _canExecute?.Invoke() ?? true;
    }

    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
    public void Execute(object? parameter)
    {
      _execute?.Invoke(parameter);
    }

    #endregion ICommand
  }

  /// <summary>
  /// Implements an <see cref="System.Windows.Input.ICommand"/> by storing and executing actions for <see cref="System.Windows.Input.ICommand.Execute(object)"/>
  /// and <see cref="System.Windows.Input.ICommand.CanExecute(object)"/>. Note that you have to manually call <see cref="OnCanExecuteChanged"/> if <see cref="CanExecute(object)"/> will return a different value than before.
  /// </summary>
  /// <seealso cref="System.Windows.Input.ICommand" />
  public class RelayCommand<T> : System.Windows.Input.ICommand
  {
    private readonly Action<T> _execute;
    private readonly Predicate<T>? _canExecute;

    #region Constructors

    public RelayCommand(Action<T> execute)
      : this(execute, null)
    {
    }

    public RelayCommand(Action<T> execute, Predicate<T>? canExecute)
    {
      _execute = execute ?? throw new ArgumentNullException(nameof(execute));
      _canExecute = canExecute;
    }

    #endregion Constructors

    #region ICommand

    /// <summary>
    /// Occurs when changes occur that affect whether or not the command should execute.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// If called, will raise the <see cref="CanExecuteChanged"/> event. Call this function manually if <see cref="CanExecute(object)"/> will return a different value than before.
    /// </summary>
    public void OnCanExecuteChanged()
    {
      CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Used to connect to the Wpf CommandManager's RequerySuggested event that is fired if something in the Gui has changed.
    /// </summary>
    /// <param name="sender">The sender (unused).</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data (unused).</param>
    public void EhRequerySuggested(object? sender, EventArgs e)
    {
      OnCanExecuteChanged();
    }

    /// <summary>
    /// Defines the method that determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
    /// <returns>
    /// true if this command can be executed; otherwise, false.
    /// </returns>
    public bool CanExecute(object? parameter)
    {
      if (_canExecute is null)
      {
        return true;
      }
      else if (parameter is T tpara)
      {
        return _canExecute.Invoke(tpara);
      }
      else if (parameter is null && default(T) is null)
      {
        return _canExecute.Invoke(default(T));
      }
      else
      {
        throw new ArgumentException("Argument is expected to be of type " + typeof(T).ToString(), nameof(parameter));
      }
    }

    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
    public void Execute(object? parameter)
    {
      if (parameter is T tpara)
      {
        _execute(tpara);
      }
      else if (parameter is null && default(T) is null)
      {
        _execute.Invoke(default(T));
      }
      else
      {
        throw new ArgumentException("Argument is expected to be of type " + typeof(T).ToString(), nameof(parameter));
      }
    }

    #endregion ICommand
  }
}
