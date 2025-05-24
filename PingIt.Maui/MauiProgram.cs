using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Maps;
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
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            if (DeviceInfo.Platform == DevicePlatform.Android
                || DeviceInfo.Platform == DevicePlatform.iOS)
            {
                builder.UseMauiMaps();
            }

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // Register HTTP clients
            builder.Services.AddHttpClient("PingItClient", client =>
            {
                client.BaseAddress = DeviceInfo.Platform == DevicePlatform.Android
                    ? new Uri("http://10.0.2.2:5276/")  // emulator → host machine
                    : new Uri("https://localhost:7017/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            builder.Services.AddHttpClient("AuthenticatedClient", client =>
            {
                client.BaseAddress = DeviceInfo.Platform == DevicePlatform.Android
                    ? new Uri("http://10.0.2.2:5276/")
                    : new Uri("https://localhost:7017/");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddTransient<AuthHeaderHandler>();
            builder.Services.AddPingItServices();

            return builder.Build();
        }
    }
}
