using PingIt.Maui.ViewModels;
using Microsoft.Maui.Controls;

namespace PingIt.Maui.Views
{
    public partial class ReportIncidentPage : ContentPage
    {
        public ReportIncidentPage(ReportIncidentViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is ReportIncidentViewModel vm && vm.UseCurrentLocationCommand.CanExecute(null))
            {
                await vm.UseCurrentLocationAsync();
            }
        }
    }
}
