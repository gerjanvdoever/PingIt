using Microsoft.AspNetCore.Components;
using PingIt.Shared.Dtos;
using System.Threading.Tasks;

namespace PingIt.Web.Pages
{
    public partial class Login
    {
        [Inject] private IAuthService Auth { get; set; } = default!;
        [Inject] private NavigationManager Nav { get; set; } = default!;

        private readonly LoginDto dto = new();
        private string? error;

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
