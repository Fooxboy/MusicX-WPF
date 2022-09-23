using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MusicX.Converters
{
    public class InversionBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           if(value is bool property)
           {
                if (property) return Visibility.Collapsed;

                return Visibility.Visible;
           }

            throw new Exception("Поддерживается только bool");

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
