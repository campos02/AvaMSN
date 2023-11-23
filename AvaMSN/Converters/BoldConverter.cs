using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace AvaMSN.Converters;

public class BoldConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool bold)
        {
            if (bold)
                return FontWeight.ExtraBold;
        }

        return FontWeight.Regular;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
