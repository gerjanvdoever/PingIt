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
            return new AuthenticationState(
                new ClaimsPrincipal(new ClaimsIdentity()));

        // 1. Decode the JWT
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // 2. Normalise “role” → ClaimTypes.Role so AuthorizeView works
        var claims = jwt.Claims.Select(c =>
            c.Type is "Role"
                ? new Claim(ClaimTypes.Role, c.Value)
                : c);

        // 3. Build the identity
        var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }


    public void NotifyUserAuthentication(string token)
        => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    public void NotifyUserLogout()
        => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
