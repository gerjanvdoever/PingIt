using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using PingIt.Web;
using PingIt.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 1. Local storage & Blazor auth
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<TokenAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<TokenAuthenticationStateProvider>());

// 2. Register our handler
builder.Services.AddTransient<AuthHeaderHandler>();

// 3. Configure a named client that uses it
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7017/");
})
.AddHttpMessageHandler<AuthHeaderHandler>();

// 4. Make the default HttpClient resolve to that named client
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>()
      .CreateClient("ApiClient"));

// 5. Your auth wrapper
builder.Services.AddScoped<IAuthService, AuthService>();

await builder.Build().RunAsync();
