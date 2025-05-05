using System.Net.Http.Json;
using System.Windows.Input;
using PingIt.Maui.Dtos;
using PingIt.Maui.Services;
using Microsoft.Extensions.Logging;
using PingIt.Maui.Views;

namespace PingIt.Maui.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly TokenStorageService _tokenStorageService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LoginViewModel> _logger;

        private string email = string.Empty;
        public string Email
        {
            get => email;
            set => SetProperty(ref email, value);
        }

        private string password = string.Empty;
        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        public string ValidationError { get => validationError; set => SetProperty(ref validationError, value); }
        private string validationError = string.Empty;

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand AnonymousCommand { get; }

        public LoginViewModel(
            TokenStorageService tokenStorageService,
            IHttpClientFactory httpClientFactory,
            ILogger<LoginViewModel> logger)
        {
            _tokenStorageService = tokenStorageService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            LoginCommand = new Command(async () => await OnLoginAsync(), () => !IsBusy);
            RegisterCommand = new Command(async () => await OnRegister());
            AnonymousCommand = new Command(OnAnonymous);
        }

        private async Task OnLoginAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ((Command)LoginCommand).ChangeCanExecute();

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
                ((Command)LoginCommand).ChangeCanExecute();
            }
        }


        private async Task OnRegister()
        {
            await Shell.Current.GoToAsync("//RegisterPage");
        }
        private void OnAnonymous()
        {
            _logger.LogInformation("Navigating anonymously...");
            // Navigate to anonymous screen (future)
        }
    }
}