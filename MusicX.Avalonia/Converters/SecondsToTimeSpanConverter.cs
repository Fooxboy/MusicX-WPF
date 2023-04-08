using System.Globalization;
using Avalonia.Data.Converters;

namespace MusicX.Avalonia.Converters;

public class SecondsToTimeSpanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            int intValue => TimeSpan.FromSeconds(intValue),
            double doubleValue => TimeSpan.FromSeconds(doubleValue),
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TimeSpan timeSpan)
            return timeSpan.TotalSeconds;
        return null;
    }
}