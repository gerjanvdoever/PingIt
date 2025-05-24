using System.Windows.Input;

namespace PingIt.Maui.Views;

public partial class BottomNavBar : ContentView
{
    public BottomNavBar()
    {
        InitializeComponent();
    }

    public static readonly BindableProperty CurrentPageProperty =
        BindableProperty.Create(nameof(CurrentPage), typeof(string), typeof(BottomNavBar), string.Empty);

    public bool AllowMap => DeviceInfo.Platform == DevicePlatform.Android;

    public string CurrentPage
    {
        get => (string)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public ICommand NavigateMapCommand => new Command(async () =>
    {
        if (CurrentPage != "IncidentMapPage")
            await Shell.Current.GoToAsync("//IncidentMapPage");
    });

    public ICommand NavigateListCommand => new Command(async () =>
    {
        if (CurrentPage != "IncidentListPage")
            await Shell.Current.GoToAsync("//IncidentListPage");
    });

    public ICommand NavigateAccountCommand => new Command(async () =>
    {
        if (CurrentPage != "AccountPage")
            await Shell.Current.GoToAsync("//AccountPage");
    });
}