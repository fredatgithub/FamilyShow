using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Microsoft.FamilyShow
{
  public class ImageConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      try
      {
        BitmapImage bitmap = new BitmapImage(new Uri(value.ToString()));

        // Use BitmapCacheOption.OnLoad to prevent binding the source holding on to the photo file.
        bitmap.CacheOption = BitmapCacheOption.OnLoad;

        return bitmap;
      }
      catch
      {
        return string.Empty;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return new object();
    }

    #endregion
  }
}
