using PingIt.Maui.Views;

namespace PingIt.Maui
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(MyIncidentDetail), typeof(MyIncidentDetail));
            Routing.RegisterRoute(nameof(ReportIncidentPage), typeof(ReportIncidentPage));
            Routing.RegisterRoute(nameof(IncidentDetailPage), typeof(IncidentDetailPage));
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
            Routing.RegisterRoute(nameof(MyAccountDetailPage), typeof(MyAccountDetailPage));
        }
    }
}
