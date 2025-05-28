using PingIt.Maui.ViewModels;

namespace PingIt.Maui.Views;

public partial class MyIncidentDetailWindowsPage : ContentPage
{
	public MyIncidentDetailWindowsPage(MyIncidentDetailViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}