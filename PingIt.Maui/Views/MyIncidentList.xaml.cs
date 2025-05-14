using PingIt.Maui.ViewModels;

namespace PingIt.Maui.Views;

public partial class MyIncidentList : ContentPage
{
	public MyIncidentList(MyIncidentListViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ((MyIncidentListViewModel)BindingContext).LoadIncidentsAsync();
    }
}