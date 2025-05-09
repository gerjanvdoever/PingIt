using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PingIt.Web;
using System.Net.Http.Headers;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using PingIt.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddTransient<AuthHeaderHandler>();   // NEW

builder.Services.AddScoped(sp =>
{
    var client = new HttpClient(
                     sp.GetRequiredService<AuthHeaderHandler>())       // inject handler
    {
        BaseAddress = new Uri("https://localhost:7017/")               // your API
    };
    return client;
});

// 2. Local storage & authentication
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<TokenAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
        p => p.GetRequiredService<TokenAuthenticationStateProvider>());

// 3. Your auth wrapper
builder.Services.AddScoped<IAuthService, AuthService>();

await builder.Build().RunAsync();

