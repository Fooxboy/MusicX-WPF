using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MusicX.Converters;

public class CountToVisibillityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int count)
            return Visibility.Visible;

        if (parameter is bool inversion && inversion)
            return count > 0 ? Visibility.Visible : Visibility.Collapsed;

        return count > 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
