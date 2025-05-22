using PingIt.Maui.ViewModels;

namespace PingIt.Maui.Views;

public partial class RegisterPage : ContentPage
{
	public RegisterPage(RegisterViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

    void OnNotificationsTapped(object sender, TappedEventArgs e)
    {
        if (BindingContext is RegisterViewModel vm)
            vm.WantsNotifications = !vm.WantsNotifications;
    }
}