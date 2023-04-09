using System.Globalization;
using Avalonia.Data.Converters;

namespace MusicX.Avalonia.Converters;

public class ObjectEqualsConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        return values.Aggregate((a, b) => a?.Equals(b) is true);
    }
}