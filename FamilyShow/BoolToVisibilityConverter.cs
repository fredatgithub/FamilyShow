using System;
using System.Windows.Data;
using System.Windows;

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

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException(Properties.Resources.NotImplemented);
    }

    #endregion
  }
}
