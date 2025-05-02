using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using PingIt.Maui.Services;
using PingIt.Shared.Dtos;
using PingIt.Shared.Enums;

namespace PingIt.Maui.ViewModels
{
    public class AccountViewModel : BaseViewModel
    {
        private readonly TokenStorageService _tokenStorage;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AccountViewModel> _logger;

        public string FirstName { get; set; } = "Demo";
        public string LastName { get; set; } = "Gebruiker";
        public UserRole Role { get; set; } = UserRole.Resident;
        public string FullName => $"{FirstName} {LastName}";

        public ICommand LogoutCommand { get; }

        public AccountViewModel(
            TokenStorageService tokenStorage,
            IHttpClientFactory httpClientFactory,
            ILogger<AccountViewModel> logger)
        {
            _tokenStorage = tokenStorage;
            _httpClient = httpClientFactory.CreateClient("AuthenticatedClient");
            _logger = logger;

            LogoutCommand = new Command(async () => await OnLogout());
        }

        public async Task OnAppearing()
        {
            await LoadUserAsync();
        }
        private async Task LoadUserAsync()
        {
            try
            {
                var userId = _tokenStorage.UserId;
                if (userId == null)
                {
                    _logger.LogWarning("No user ID found in token storage");
                    return;
                }

                var response = await _httpClient.GetAsync($"api/user/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to load user info. Status: {StatusCode}", response.StatusCode);
                    return;
                }
                var rawResponse = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Raw API response: {RawJson}", rawResponse);

                var user = await response.Content.ReadFromJsonAsync<UserDto>();
                if (user != null)
                {
                    FirstName = user.FirstName;
                    LastName = user.LastName;
                    Role = user.Role;
                    OnPropertyChanged(nameof(FullName));
                    OnPropertyChanged(nameof(Role));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user");
            }
        }

        private async Task OnLogout()
        {
            await _tokenStorage.ClearTokenAsync();
            await Shell.Current.GoToAsync("//login");
        }
    }
}