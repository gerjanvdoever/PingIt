using Blazored.LocalStorage;
using System.Net.Http.Json;
using PingIt.Shared.Enums;
using PingIt.Shared.Dtos;
using Microsoft.AspNetCore.Components.Authorization;
using PingIt.Maui.Dtos;

public interface IAuthService
{
    Task<bool> LoginAsync(LoginDto dto);
    Task LogoutAsync();
}

public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly ILocalStorageService _storage;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(HttpClient http, ILocalStorageService storage,
                       AuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _storage = storage;
        _authStateProvider = authStateProvider;
    }

    public async Task<bool> LoginAsync(LoginDto dto)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/login", dto);

        if (!resp.IsSuccessStatusCode) return false;

        var loginResp =
            await resp.Content.ReadFromJsonAsync<LoginResponseDto>();

        // Reject non-admins
        bool isAdmin = loginResp!.Role == "Administrator";

        if (!isAdmin)
            return false;

        // Persist token and notify auth pipeline
        await _storage.SetItemAsync("authToken", loginResp.Token);
        ((TokenAuthenticationStateProvider)_authStateProvider)
            .NotifyUserAuthentication(loginResp.Token);

        return true;
    }

    public async Task LogoutAsync()
    {
        await _storage.RemoveItemAsync("authToken");
        ((TokenAuthenticationStateProvider)_authStateProvider)
            .NotifyUserLogout();
    }
}
