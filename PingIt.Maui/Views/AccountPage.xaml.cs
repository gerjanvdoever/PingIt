using PingIt.Maui.ViewModels;

namespace PingIt.Maui.Views;

public partial class AccountPage : ContentPage
{
    public AccountPage(AccountViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}