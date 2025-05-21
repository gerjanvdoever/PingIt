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
using PingIt.Maui.Views;

namespace PingIt.Maui.ViewModels
{
    public partial class AccountViewModel : ObservableObject
    {
        private readonly TokenStorageService _tokenStorage;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AccountViewModel> _logger;

        public bool AllowReport => DeviceInfo.Platform == DevicePlatform.Android;

        [ObservableProperty]
        private string firstName = "Fetching user info...";

        [ObservableProperty]
        private string lastName = "";

        [ObservableProperty]
        private UserRole role = UserRole.Resident;

        [ObservableProperty]
        private bool isFooterVisible;

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

        [RelayCommand]
        private async Task NavigateReport()
        {
            await Shell.Current.GoToAsync(nameof(ReportIncidentPage));
        }

        [RelayCommand]
        private async Task NavigateList()
        {
            await Shell.Current.GoToAsync(nameof(MyIncidentList));
        }

        private async Task LoadUserAsync()
        {
            try
            {
                var userId = _tokenStorage.UserId;
                if (userId == null)
                {
                    await Logout();
                }

                var response = await _httpClient.GetAsync($"api/user/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to load user info. Status: {StatusCode}", response.StatusCode);
                    await Logout();
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

                    IsFooterVisible = Role == UserRole.Administrator || Role == UserRole.Worker;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user");
            }
        }
    }
}