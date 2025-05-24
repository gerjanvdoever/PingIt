using System.Globalization;

namespace PingIt.Maui.Converters;

public class PageToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string currentPage && parameter is string targetPage)
        {
            // Return a light grey background color if this is the current page
            if (currentPage == targetPage)
            {
                return Application.Current.RequestedTheme == AppTheme.Dark
                    ? Color.FromArgb("#333333") // Dark grey for dark theme
                    : Color.FromArgb("#E0E0E0"); // Light grey for light theme
            }
        }

        return Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}