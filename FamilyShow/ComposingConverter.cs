using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace Microsoft.FamilyShow
{
  public class ComposingConverter : IValueConverter
  {
    #region IValueCOnverter Members

    private List<IValueConverter> converters = new List<IValueConverter>();

    public Collection<IValueConverter> Converters
    {
      get { return new Collection<IValueConverter>(converters); }
    }

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      for (int i = 0; i < converters.Count; i++)
      {
        value = converters[i].Convert(value, targetType, parameter, culture);
      }

      return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      for (int i = converters.Count - 1; i >= 0; i--)
      {
        value = converters[i].ConvertBack(value, targetType, parameter, culture);
      }

      return value;
    }

    #endregion
  }
}
