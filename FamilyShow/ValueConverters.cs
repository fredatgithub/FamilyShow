using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media.Imaging;

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
        return ((DateTime)value).ToShortDateString();

      return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      // Ignore empty strings. this will cause the binding to bypass validation.
      if (string.IsNullOrEmpty((string)value))
        return Binding.DoNothing;

      string dateString = (string)value;

      // Append first month and day if just the year was entered
      if (dateString.Length == 4)
        dateString = "1/1/" + dateString;

      DateTime date;
      DateTime.TryParse(dateString, out date);
      return date;
    }

    #endregion
  }

  /// <summary>
  /// This converter is used to show possessive first name. Note: doesn't handle names that end in 's' correctly yet.
  /// </summary>
  public class FirstNamePossessiveFormConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value != null)
      {
        // Simply add "'s". Can be extended to check for correct grammar.
        return value.ToString() + "'s ";
      }

      return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException(Properties.Resources.NotImplemented);
    }

    #endregion
  }

  /// <summary>
  /// This converter is used to show the "*" on the picture when the IsAvatar property is true.
  /// </summary>
  public class PrimaryAvatarConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value != null && (bool)value)
        return "*";

      return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException(Properties.Resources.NotImplemented);
    }

    #endregion
  }

  public class BoolToVisibilityConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if ((bool)value)
        return Visibility.Visible;
      else
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException(Properties.Resources.NotImplemented);
    }

    #endregion
  }

  public class NotConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      return !(bool)value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException(Properties.Resources.NotImplemented);
    }

    #endregion
  }

  public class ComposingConverter : IValueConverter
  {
    #region IValueCOnverter Members

    private List<IValueConverter> converters = new List<IValueConverter>();

    public Collection<IValueConverter> Converters
    {
      get { return new Collection<IValueConverter>(this.converters); }
    }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      for (int i = 0; i < this.converters.Count; i++)
      {
        value = converters[i].Convert(value, targetType, parameter, culture);
      }
      return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      for (int i = this.converters.Count - 1; i >= 0; i--)
      {
        value = converters[i].ConvertBack(value, targetType, parameter, culture);
      }
      return value;
    }

    #endregion
  }

  public class ImageConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
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
        return "";
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException(Properties.Resources.NotImplemented);
    }

    #endregion
  }
}
