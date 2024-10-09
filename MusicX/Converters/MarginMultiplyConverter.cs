using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MusicX.Converters;

public class MarginMultiplyConverter : IValueConverter
{
    public Thickness Margin { get; set; }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int index)
            return new Thickness(index * Margin.Left, index * Margin.Top, index * Margin.Right, index * Margin.Bottom);

        return Margin;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}