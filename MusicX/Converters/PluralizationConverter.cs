using System;
using System.Globalization;
using System.Windows.Data;

namespace MusicX.Converters;

public class PluralizationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not int count)
            throw new ArgumentException(null, nameof(value));
        if (parameter is not string s)
            throw new ArgumentException(null, nameof(parameter));

        return count % 10 is 2 or 3 or 4 ? s + "а" : s;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}