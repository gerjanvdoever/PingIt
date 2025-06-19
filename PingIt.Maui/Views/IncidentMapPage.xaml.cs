using PingIt.Maui.ViewModels;

namespace PingIt.Maui.Views;

public partial class IncidentMapPage : ContentPage
{
    private readonly IncidentMapViewModel _vm;
    public IncidentMapPage(IncidentMapViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_vm.LoadIncidentsCommand.CanExecute(null))
        {
            _vm.LoadIncidentsCommand.Execute(null);
        }

    }
}
