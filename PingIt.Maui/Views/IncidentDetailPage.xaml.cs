using PingIt.Maui.ViewModels;

namespace PingIt.Maui.Views;

public partial class IncidentDetailPage : ContentPage
{
	public IncidentDetailPage(IncidentDetailViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}