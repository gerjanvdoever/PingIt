using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace PingIt.Maui.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        // value: the bound bool
        // parameter: a string like "Hide Closed Incidents,Show Closed Incidents"
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool flag && parameter is string param)
            {
                var parts = param.Split(',');
                if (parts.Length >= 2)
                    return flag ? parts[0].Trim() : parts[1].Trim();
            }

            // fallback
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is not implemented for BoolToStringConverter.");
        }
    }
}
