using PingIt.Maui.ViewModels;

namespace PingIt.Maui.Views;

public partial class IncidentDetailWindowsPage : ContentPage
{
	public IncidentDetailWindowsPage(IncidentDetailViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}