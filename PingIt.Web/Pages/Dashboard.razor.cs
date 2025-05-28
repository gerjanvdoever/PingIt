using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using PingIt.Web.Pages.Base;

namespace PingIt.Web.Pages
{
    public partial class Dashboard
    {
        [Inject]
        private IAuthService AuthService { get; set; }

        private async Task Logout()
        {
            await AuthService.LogoutAsync();
            Nav.NavigateTo("/login", replace: true);
        }
    }
}
