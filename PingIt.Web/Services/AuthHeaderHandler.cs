using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace PingIt.Web.Services
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
            var token = (await _storage.GetItemAsStringAsync("authToken"))?.Trim('"');

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
                Console.WriteLine("Authorization header set.");
            }
            else
            {
                Console.WriteLine("No token found!");
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _storage.RemoveItemAsync("authToken");
                _nav.NavigateTo("/login", replace: true);
            }

            return response;
        }
    }
}
