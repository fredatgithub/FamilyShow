using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.FamilyShow
{
  public class BoolToVisibilityConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if ((bool)value)
      {
        return Visibility.Visible;
      }
      else
      {
        return Visibility.Collapsed;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      // not implemented yet
      return new object();
    }

    #endregion
  }
}
