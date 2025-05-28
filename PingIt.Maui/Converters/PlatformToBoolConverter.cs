using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingIt.Maui.Converters
{
    public class PlatformToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DevicePlatform platform && parameter is string targetPlatform)
            {
                return platform.ToString().Equals(targetPlatform, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
