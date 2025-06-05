using System.Net.Http.Json;
using System.Windows.Input;
using PingIt.Maui.Dtos;
using PingIt.Maui.Services;
using Microsoft.Extensions.Logging;
using PingIt.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;

namespace PingIt.Maui.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly ITokenStorageService _tokenStorageService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LoginViewModel> _logger;

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private string validationError = string.Empty;

        [ObservableProperty]
        private bool isBusy;

        public bool IsNotBusy => !IsBusy;

        public LoginViewModel(
            ITokenStorageService tokenStorageService,
            IHttpClientFactory httpClientFactory,
            ILogger<LoginViewModel> logger)
        {
            _tokenStorageService = tokenStorageService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private async Task LoginAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            LoginCommand.NotifyCanExecuteChanged();

            ValidationError = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
                {
                    ValidationError = "Please fill in all required fields";
                    return;
                }

                var client = _httpClientFactory.CreateClient("PingItClient");
                var loginDto = new { Email, Password };

                var response = await client.PostAsJsonAsync("api/auth/login", loginDto);

                if (!response.IsSuccessStatusCode)
                {
                    var errorJson = await response.Content.ReadAsStringAsync();

                    try
                    {
                        var errorObj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(errorJson);
                        if (errorObj != null && errorObj.TryGetValue("Message", out var message))
                        {
                            ValidationError = message;
                        }
                        else
                        {
                            ValidationError = "Login failed";
                        }
                    }
                    catch
                    {
                        ValidationError = "Login failed";
                    }

                    return;
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                if (result == null || string.IsNullOrEmpty(result.Token))
                {
                    ValidationError = "Didn't receive valid token";
                    return;
                }

                await _tokenStorageService.StoreTokenAsync(result.Token, result.Role);
                await Shell.Current.GoToAsync("//AccountPage");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                ValidationError = "Something went wrong when trying to log in.";
            }
            finally
            {
                IsBusy = false;
                LoginCommand.NotifyCanExecuteChanged();
            }
        }

        [RelayCommand]
        private async Task Register()
        {
            IsBusy = true;
            try
            {
                await Shell.Current.GoToAsync(nameof(RegisterPage));
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task Anonymous()
        {
            IsBusy = true;
            try
            {
                if (DeviceInfo.Platform != DevicePlatform.WinUI)
                {
                    await Shell.Current.GoToAsync(nameof(ReportIncidentPage));
                }
                else
                {
                    var window = Application.Current?.Windows.FirstOrDefault();
                    var currentPage = window?.Page;

                    if (currentPage != null)
                    {
                        await currentPage.DisplayAlert(
                            "Notice",
                            "This feature is not available on Windows devices.",
                            "OK");
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}