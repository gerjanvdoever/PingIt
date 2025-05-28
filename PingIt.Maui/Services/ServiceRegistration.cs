using Microsoft.Extensions.DependencyInjection;
using PingIt.Maui.Services;
using PingIt.Maui.ViewModels;
using PingIt.Maui.Views;

namespace PingIt.Maui;

public static class ServiceRegistration
{
    public static IServiceCollection AddPingItServices(this IServiceCollection services)
    {
        // Services
        services.AddSingleton<TokenStorageService>();
        services.AddSingleton<IIncidentStore, IncidentStore>();
        services.AddSingleton<IUserStore, UserStore>();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<AccountViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<ReportIncidentViewModel>();
        services.AddTransient<IncidentListViewModel>();
        services.AddTransient<MyIncidentDetailViewModel>();
        services.AddTransient<IncidentMapViewModel>();
        services.AddTransient<IncidentDetailViewModel>();
        services.AddTransient<MyAccountDetailViewModel>();

        // Views
        services.AddTransient<LoginPage>();
        services.AddTransient<AccountPage>();
        services.AddTransient<RegisterPage>();
        services.AddTransient<ReportIncidentPage>();
        services.AddTransient<IncidentListPage>();
        services.AddTransient<BottomNavBar>();
        services.AddTransient<TopNavBar>();
        services.AddTransient<MyIncidentDetail>();
        services.AddTransient<IncidentMapPage>();
        services.AddTransient<IncidentDetailPage>();
        services.AddTransient<IncidentDetailWindowsPage>();
        services.AddTransient<MyAccountDetailPage>();

        return services;
    }
}
