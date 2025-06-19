using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PingIt.Shared.Dtos;
using PingIt.Shared.Enums;
using PingIt.Web.Services;

namespace PingIt.Web.Pages
{
public partial class AdminPanel
{
    [Inject] private IUserService UserService { get; set; }
    [Inject] private IJSRuntime JS { get; set; }

    private List<UserDto> allUsers = new();
    private List<UserDto> filteredUsers = new();
    private const int MaxVisible = 40;

    private string _firstNameFilter = "";
    private string _lastNameFilter = "";
    private string _emailFilter = "";
    private string _streetFilter = "";
    private string _postalFilter = "";
    private string _cityFilter = "";

    private string FirstNameFilter
    {
        get => _firstNameFilter;
        set { _firstNameFilter = value; ApplyFilters(); }
    }

    private string LastNameFilter
    {
        get => _lastNameFilter;
        set { _lastNameFilter = value; ApplyFilters(); }
    }

    private string EmailFilter
    {
        get => _emailFilter;
        set { _emailFilter = value; ApplyFilters(); }
    }

    private string StreetFilter
    {
        get => _streetFilter;
        set { _streetFilter = value; ApplyFilters(); }
    }

    private string PostalFilter
    {
        get => _postalFilter;
        set { _postalFilter = value; ApplyFilters(); }
    }

    private string CityFilter
    {
        get => _cityFilter;
        set { _cityFilter = value; ApplyFilters(); }
    }

    protected override async Task OnInitializedAsync()
    {
        allUsers = await UserService.GetAllUsersAsync();
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        filteredUsers = allUsers
            .Where(u =>
                u.FirstName.Contains(_firstNameFilter, StringComparison.OrdinalIgnoreCase) &&
                u.LastName.Contains(_lastNameFilter, StringComparison.OrdinalIgnoreCase) &&
                u.Email.Contains(_emailFilter, StringComparison.OrdinalIgnoreCase) &&
                u.Street.Contains(_streetFilter, StringComparison.OrdinalIgnoreCase) &&
                u.PostalCode.Contains(_postalFilter, StringComparison.OrdinalIgnoreCase) &&
                u.City.Contains(_cityFilter, StringComparison.OrdinalIgnoreCase)
            )
            .Take(MaxVisible)
            .ToList();
    }

    private async Task OnRoleChanged(UserDto user, ChangeEventArgs e)
    {
        if (Enum.TryParse<UserRole>(e.Value?.ToString(), out var newRole) && newRole != user.Role)
        {
            var oldRole = user.Role;
            user.Role = newRole;

            var success = await UserService.UpdateUserRoleAsync(user.Id, newRole);
            if (!success)
            {
                user.Role = oldRole;
                await JS.InvokeVoidAsync("alert", "Failed to update role.");
            }
        }
    }
}
}
