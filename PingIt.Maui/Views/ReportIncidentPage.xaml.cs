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
    }
}
