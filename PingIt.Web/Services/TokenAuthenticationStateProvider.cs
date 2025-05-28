using Blazored.LocalStorage;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;

public class TokenAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _storage;

    public TokenAuthenticationStateProvider(ILocalStorageService storage)
        => _storage = storage;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _storage.GetItemAsStringAsync("authToken");

        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        if (token.StartsWith("\"") && token.EndsWith("\""))
            token = token.Trim('"');

        try
        {
            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
            {
                await _storage.RemoveItemAsync("authToken");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var jwt = handler.ReadJwtToken(token);

            if (jwt.ValidTo < DateTime.UtcNow)
            {
                await _storage.RemoveItemAsync("authToken");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = jwt.Claims.Select(c =>
                (c.Type == "Role" || c.Type == "role")
                    ? new Claim(ClaimTypes.Role, c.Value)
                    : c);

            var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch (Exception)
        {
            await _storage.RemoveItemAsync("authToken");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }



    public void NotifyUserAuthentication(string token)
        => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    public void NotifyUserLogout()
        => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
