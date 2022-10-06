using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MusicX.Converters
{
    public class ListToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is IList list)
            {
                var val = list.Count > 0;

                var inversion = bool.Parse(parameter as string);

                if (inversion) val = !val;

                if (val) return Visibility.Visible;
                else return Visibility.Collapsed;
            }
            else
            {
                throw new Exception("Поддерживается только коллекции реализующие IList");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
