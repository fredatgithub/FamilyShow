using System;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.FamilyShow
{
  /// <summary>
  /// This converter is used to show DateTime in short date format
  /// </summary>
  public class DateFormattingConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value != null)
      {
        return ((DateTime)value).ToShortDateString();
      }

      return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      // Ignore empty strings. this will cause the binding to bypass validation.
      if (string.IsNullOrEmpty((string)value))
      {
        return Binding.DoNothing;
      }

      string dateString = (string)value;

      // Append first month and day if just the year was entered
      if (dateString.Length == 4)
      {
        dateString = "1/1/" + dateString;
      }

      _ = DateTime.TryParse(dateString, out DateTime date);
      return date;
    }

    #endregion
  }
}