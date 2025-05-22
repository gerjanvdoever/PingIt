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
        await ViewModel.InitializeAsync();
    }
}