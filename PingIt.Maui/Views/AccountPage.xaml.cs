using PingIt.Maui.ViewModels;

namespace PingIt.Maui.Views;

public partial class AccountPage : ContentPage
{
    private AccountViewModel ViewModel => (AccountViewModel)BindingContext;

    public AccountPage(AccountViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        RootContent.Opacity = 0;
        RootContent.TranslationY = 1000;

        await Task.Delay(50);

        var fadeTask = RootContent.FadeTo(1, 300);
        var slideTask = RootContent.TranslateTo(0, 0, 400, Easing.CubicOut);

        await Task.WhenAll(fadeTask, slideTask);
        await ViewModel.InitializeAsync();
    }
}