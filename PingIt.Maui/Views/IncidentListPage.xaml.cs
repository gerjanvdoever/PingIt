using PingIt.Maui.ViewModels;

namespace PingIt.Maui.Views;

public partial class IncidentListPage : ContentPage
{
    private readonly IncidentListViewModel _vm;

    public IncidentListPage(IncidentListViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadIncidentsAsync();
    }
}
