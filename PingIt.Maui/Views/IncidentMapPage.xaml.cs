using PingIt.Maui.ViewModels;

namespace PingIt.Maui.Views;

public partial class IncidentMapPage : ContentPage
{
    public IncidentMapPage(IncidentMapViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is IncidentMapViewModel vm &&
            vm.LoadIncidentsCommand.CanExecute(null))
        {
            vm.LoadIncidentsCommand.Execute(null);
        }
    }
}
