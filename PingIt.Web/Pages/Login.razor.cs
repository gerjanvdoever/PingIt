using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using PingIt.Shared.Dtos;
using System.Threading.Tasks;

namespace PingIt.Web.Pages
{
    public partial class Login
    {
        [Inject] private IAuthService Auth { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;

        private readonly LoginDto dto = new();
        private string? error;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            // If already logged in, redirect to dashboard
            if (user.Identity?.IsAuthenticated == true && user.IsInRole("Administrator"))
            {
                Nav.NavigateTo("/dashboard", replace: true);
            }
        }
        private async Task HandleLogin()
        {
            var ok = await Auth.LoginAsync(dto);
            if (!ok)
            {
                error = "Invalid credentials or no permission";
                return;
            }

            Nav.NavigateTo("/dashboard", replace: true);
        }
    }
}
