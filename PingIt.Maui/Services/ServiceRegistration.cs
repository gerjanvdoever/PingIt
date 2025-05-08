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

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<AccountViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<ReportIncidentViewModel>();
        services.AddTransient<IncidentListViewModel>();

        // Views
        services.AddTransient<LoginPage>();
        services.AddTransient<AccountPage>();
        services.AddTransient<RegisterPage>();
        services.AddTransient<ReportIncidentPage>();
        services.AddTransient<IncidentListPage>();
        services.AddTransient<BottomNavBar>();

        return services;
    }
}
