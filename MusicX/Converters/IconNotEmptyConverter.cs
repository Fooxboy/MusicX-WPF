using System;
using System.Windows.Data;
using Wpf.Ui.Controls;

namespace MusicX.Converters;

/// <summary>
/// Checks if the <see cref="SymbolRegular"/> is valid and not empty.
/// </summary>
internal class IconNotEmptyConverter : IValueConverter
{
    /// <summary>
    /// Checks if the <see cref="SymbolRegular"/> is valid and not empty.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is SymbolRegular icon)
            return icon != SymbolRegular.Empty;

        return false;
    }

    /// <summary>
    /// Not Implemented.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public object ConvertBack(object value, Type targetType, object parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}