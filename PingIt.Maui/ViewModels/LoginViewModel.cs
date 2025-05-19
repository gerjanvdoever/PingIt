using System.Net.Http.Json;
using System.Windows.Input;
using PingIt.Maui.Dtos;
using PingIt.Maui.Services;
using Microsoft.Extensions.Logging;
using PingIt.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PingIt.Maui.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly TokenStorageService _tokenStorageService;
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
            TokenStorageService tokenStorageService,
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
                    ValidationError = "Vul zowel e-mailadres als wachtwoord in.";
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
                            ValidationError = "Inloggen mislukt.";
                        }
                    }
                    catch
                    {
                        ValidationError = "Inloggen mislukt.";
                    }

                    return;
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                if (result == null || string.IsNullOrEmpty(result.Token))
                {
                    ValidationError = "Geen geldig token ontvangen.";
                    return;
                }

                await _tokenStorageService.StoreTokenAsync(result.Token, result.Role);
                await Shell.Current.GoToAsync("//AccountPage");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                ValidationError = "Er trad een fout op tijdens het inloggen.";
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
            await Shell.Current.GoToAsync("//RegisterPage");
        }

        [RelayCommand]
        private async void Anonymous()
        {
            await Shell.Current.GoToAsync(nameof(ReportIncidentPage));
        }
    }
}