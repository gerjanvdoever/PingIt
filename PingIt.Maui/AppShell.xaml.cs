using PingIt.Maui.Views;

namespace PingIt.Maui
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(MyIncidentList), typeof(MyIncidentList));
            Routing.RegisterRoute(nameof(MyIncidentDetail), typeof(MyIncidentDetail));
        }
    }
}
