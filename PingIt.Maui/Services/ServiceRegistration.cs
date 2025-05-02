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

        // Views
        services.AddTransient<LoginPage>();
        services.AddTransient<AccountPage>();

        return services;
    }
}
