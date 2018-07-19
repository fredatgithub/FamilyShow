using System;
using System.Windows.Data;

namespace Microsoft.FamilyShow
{
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
}