using System.Globalization;

namespace PingIt.Maui.Converters;

public class PageToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var currentPage = value?.ToString();
        var buttonPage = parameter?.ToString();

        if (currentPage == buttonPage)
            return Colors.Gray;

        return Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
