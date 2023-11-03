using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace AvaMSN.Converters;

public class UserSaysConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isHistory)
        {
            if (isHistory)
                return "said:";

            else
                return "says:";
        }

        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
