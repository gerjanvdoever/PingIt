using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace PingIt.Web.Services   // keep the namespace that matches your project
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly ILocalStorageService _storage;
        private readonly NavigationManager _nav;

        public AuthHeaderHandler(ILocalStorageService storage, NavigationManager nav)
        {
            _storage = storage;
            _nav = nav;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1. Paste the token (if any) into the Authorization header
            var token = await _storage.GetItemAsStringAsync("authToken");
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            var response = await base.SendAsync(request, cancellationToken);

            // 2. If the server says 401, bounce user back to /login
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _storage.RemoveItemAsync("authToken");
                _nav.NavigateTo("/login", replace: true);
            }

            return response;
        }
    }
}
