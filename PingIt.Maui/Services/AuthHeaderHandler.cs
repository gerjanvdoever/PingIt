using System.Net.Http.Headers;
using PingIt.Maui.Services;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly ITokenStorageService _tokenStorage;

    public AuthHeaderHandler(ITokenStorageService tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Add the authorization header if we have a token
        if (!string.IsNullOrEmpty(_tokenStorage.Token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _tokenStorage.Token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}