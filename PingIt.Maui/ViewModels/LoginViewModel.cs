using System.Net.Http.Json;
using System.Windows.Input;
using PingIt.Maui.Dtos;
using PingIt.Maui.Services;
using Microsoft.Extensions.Logging;

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

        public ICommand LoginCommand { get; }
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
            AnonymousCommand = new Command(OnAnonymous);
        }

        private async Task OnLoginAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ((Command)LoginCommand).ChangeCanExecute();

            try
            {
                var client = _httpClientFactory.CreateClient("PingItClient");
                var loginDto = new { Email, Password };

                var response = await client.PostAsJsonAsync("api/auth/login", loginDto);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Login failed: {Error}", error);
                    return;
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                if (result == null || string.IsNullOrEmpty(result.Token))
                {
                    _logger.LogWarning("No token received");
                    return;
                }

                await _tokenStorageService.StoreTokenAsync(result.Token, result.Role);
                await Shell.Current.GoToAsync("//AccountPage");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
            }
            finally
            {
                IsBusy = false;
                ((Command)LoginCommand).ChangeCanExecute();
            }
        }

        private void OnAnonymous()
        {
            _logger.LogInformation("Navigating anonymously...");
            // Navigate to anonymous screen (future)
        }
    }
}