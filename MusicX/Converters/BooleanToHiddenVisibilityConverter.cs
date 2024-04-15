using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MusicX.Converters;

public class BooleanToHiddenVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool flag)
            return flag ? Visibility.Visible : Visibility.Hidden;
        
        return Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Visibility.Visible;
    }
}