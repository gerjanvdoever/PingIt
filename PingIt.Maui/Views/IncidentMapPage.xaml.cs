using PingIt.Maui.ViewModels;

namespace PingIt.Maui.Views;

public partial class IncidentMapPage : ContentPage
{
	public IncidentMapPage(IncidentMapViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}