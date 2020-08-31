using System;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.FamilyShow
{
  /// <summary>
  /// This converter is used to show the "*" on the picture when the IsAvatar property is true.
  /// </summary>
  public class PrimaryAvatarConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value != null && (bool)value)
      {
        return "*";
      }

      return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      // not implemented yet
      return new object();
    }

    #endregion
  }
}
