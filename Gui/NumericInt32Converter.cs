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

#nullable disable warnings
using System;
using System.Windows.Controls;
using System.Windows.Data;

namespace VolatileDriveConnector.Gui
{
  public class NumericInt32Converter : ValidationRule, IValueConverter
  {
    private System.Globalization.CultureInfo _conversionCulture = System.Globalization.CultureInfo.CurrentUICulture;
    private string _lastConvertedString;

    private int? _lastConvertedValue;

    public int MinimumValue { get; set; }

    public int MaximumValue { get; set; }

    public NumericInt32Converter()
    {
      MinimumValue = int.MinValue;
      MaximumValue = int.MaxValue;
    }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo cultureDontUseIsBuggy)
    {
      var val = (int)value;

      if (_lastConvertedString is not null && val == _lastConvertedValue)
      {
        return _lastConvertedString;
      }

      _lastConvertedValue = val;
      _lastConvertedString = val.ToString(_conversionCulture);
      return _lastConvertedString;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo cultureDontUseIsBuggy)
    {
      var validationResult = ConvertAndValidate(value, out var result);
      if (validationResult.IsValid)
      {
        _lastConvertedString = (string)value;
        _lastConvertedValue = result;
      }
      return result;
    }

    public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureDontUseIsBuggy)
    {
      var validationResult = ConvertAndValidate(value, out var result);
      if (validationResult.IsValid)
      {
        _lastConvertedString = (string)value;
        _lastConvertedValue = result;
      }
      return validationResult;
    }

    private ValidationResult ConvertAndValidate(object value, out int result)
    {
      var s = (string)value;

      if (_lastConvertedValue is not null && s == _lastConvertedString)
      {
        result = _lastConvertedValue.Value;
        return ValidateSuccessfullyConvertedValue(result);
      }

      if (int.TryParse(s, System.Globalization.NumberStyles.Integer, _conversionCulture, out result))
      {
        return ValidateSuccessfullyConvertedValue(result);
      }

      return new ValidationResult(false, "This string could not be converted to a number");
    }

    private ValidationResult ValidateSuccessfullyConvertedValue(int result)
    {
      if (result < MinimumValue)
      {
        return new ValidationResult(false, string.Format("The entered value is less than the minimum allowed value of {0}.", MinimumValue));
      }

      if (result > MaximumValue)
      {
        return new ValidationResult(false, string.Format("The entered value is greater than the maximum allowed value of {0}.", MaximumValue));
      }

      return ValidationResult.ValidResult;
    }
  }
}
