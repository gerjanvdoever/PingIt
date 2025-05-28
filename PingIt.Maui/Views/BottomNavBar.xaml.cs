using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Devices;
using System.Windows.Input;

namespace PingIt.Maui.Views;

public partial class BottomNavBar : ContentView
{
    public BottomNavBar()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public static readonly BindableProperty CurrentPageProperty =
        BindableProperty.Create(nameof(CurrentPage), typeof(string), typeof(BottomNavBar), string.Empty);

    public string CurrentPage
    {
        get => (string)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    [RelayCommand]
    private async Task NavigateMapAsync()
    {
        if (CurrentPage != "IncidentMapPage")
        {
            if (DeviceInfo.Platform != DevicePlatform.WinUI)
            {
                await Shell.Current.GoToAsync("//IncidentMapPage");
            }
            else
            {
                var window = Application.Current?.Windows.FirstOrDefault();
                var currentPage = window?.Page;

                if (currentPage != null)
                {
                    await currentPage.DisplayAlert(
                        "Notice",
                        "This feature is not available on Windows devices.",
                        "OK");
                }
            }
        }
    }

    [RelayCommand]
    private async Task NavigateListAsync()
    {
        if (CurrentPage != "IncidentListPage")
        {
            await Shell.Current.GoToAsync("//IncidentListPage");
        }
    }

    [RelayCommand]
    private async Task NavigateAccountAsync()
    {
        if (CurrentPage != "AccountPage")
        {
            await Shell.Current.GoToAsync("//AccountPage");
        }
    }
}