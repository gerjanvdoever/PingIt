
using System;
using System.Globalization;
using Microsoft.Maui.Controls.Maps;
using PingIt.Shared.Dtos;

namespace PingIt.Maui.Converters
{
    public class DtoToLocationConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is LocationDto dto)
                return new Location((double)dto.Latitude, (double)dto.Longitude);
            return default(Location);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Location loc)
            {
                return new LocationDto
                {
                    Latitude = (decimal)loc.Latitude,
                    Longitude = (decimal)loc.Longitude
                };
            }
            return null;
        }
    }
}

