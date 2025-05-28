using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PingIt.Web.Pages.Base
{
public abstract class BaseAdminPage : ComponentBase
    {
        [Inject] protected AuthenticationStateProvider AuthProvider { get; set; } = default!;
        [Inject] protected NavigationManager Nav { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            var state = await AuthProvider.GetAuthenticationStateAsync();
            var user = state.User;

            if (!user.Identity?.IsAuthenticated ?? true || !user.IsInRole("Administrator"))
            {
                Nav.NavigateTo("/login", replace: true);
            }

            await base.OnInitializedAsync();
        }
    }
}
