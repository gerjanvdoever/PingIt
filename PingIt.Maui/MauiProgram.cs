using Microsoft.Extensions.Logging;
using PingIt.Maui.Services;

namespace PingIt.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Register HTTP clients
            builder.Services.AddHttpClient("PingItClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7017/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            builder.Services.AddHttpClient("AuthenticatedClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:7017/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            }).AddHttpMessageHandler<AuthHeaderHandler>();

            // Register AuthHeaderHandler and other PingIt services
            builder.Services.AddTransient<AuthHeaderHandler>();
            builder.Services.AddPingItServices(); // Your centralized service registration

            return builder.Build();
        }
    }
}