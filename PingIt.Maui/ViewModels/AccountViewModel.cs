using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using PingIt.Maui.Services;
using PingIt.Shared.Dtos;
using PingIt.Shared.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Data;

namespace PingIt.Maui.ViewModels
{
    public partial class AccountViewModel : ObservableObject
    {
        private readonly TokenStorageService _tokenStorage;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AccountViewModel> _logger;

        [ObservableProperty]
        private string firstName = "Demo";

        [ObservableProperty]
        private string lastName = "Gebruiker";

        [ObservableProperty]
        private UserRole role = UserRole.Resident;

        public string FullName => $"{FirstName} {LastName}";

        public AccountViewModel(
            TokenStorageService tokenStorage,
            IHttpClientFactory httpClientFactory,
            ILogger<AccountViewModel> logger)
        {
            _tokenStorage = tokenStorage;
            _httpClient = httpClientFactory.CreateClient("AuthenticatedClient");
            _logger = logger;

            _ = LoadUserAsync();
        }

        [RelayCommand]
        private async Task Logout()
        {
            await _tokenStorage.ClearTokenAsync();
            await Shell.Current.GoToAsync("//LoginPage");
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
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user");
            }
        }
    }
}