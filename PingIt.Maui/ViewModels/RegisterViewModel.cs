using System;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PingIt.Maui.Services;
using PingIt.Maui.Dtos;
using PingIt.Shared.Dtos;
using Microsoft.Maui.Controls;

namespace PingIt.Maui.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RegisterViewModel> _logger;
        private readonly ITokenStorageService _tokenStorage;

        [ObservableProperty] private string firstName = string.Empty;
        [ObservableProperty] private string lastName = string.Empty;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string password = string.Empty;
        [ObservableProperty] private string confirmPassword = string.Empty;
        [ObservableProperty] private string? phoneNumber;
        [ObservableProperty] private string street = string.Empty;
        [ObservableProperty] private string houseNumber = string.Empty;
        [ObservableProperty] private string postalCode = string.Empty;
        [ObservableProperty] private string city = string.Empty;
        [ObservableProperty] private bool wantsNotifications = false;
        [ObservableProperty] private string validationError = string.Empty;
        [ObservableProperty] private bool isBusy;

        public bool IsNotBusy => !IsBusy;

        public RegisterViewModel(
            ITokenStorageService tokenStorage,
            IHttpClientFactory httpClientFactory,
            ILogger<RegisterViewModel> logger)
        {
            _tokenStorage = tokenStorage;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task RegisterAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            RegisterCommand.NotifyCanExecuteChanged();

            try
            {
                ValidationError = string.Empty;

                if (string.IsNullOrWhiteSpace(FirstName) ||
                    string.IsNullOrWhiteSpace(LastName) ||
                    string.IsNullOrWhiteSpace(Email) ||
                    string.IsNullOrWhiteSpace(Password) ||
                    string.IsNullOrWhiteSpace(ConfirmPassword) ||
                    string.IsNullOrWhiteSpace(Street) ||
                    string.IsNullOrWhiteSpace(HouseNumber) ||
                    string.IsNullOrWhiteSpace(PostalCode) ||
                    string.IsNullOrWhiteSpace(City))
                {
                    ValidationError = "Please fill in all required fields.";
                    return;
                }

                if (Password != ConfirmPassword)
                {
                    ValidationError = "Passwords don't match";
                    return;
                }

                var passwordRegex = new Regex(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9]).{6,}$");
                if (!passwordRegex.IsMatch(Password))
                {
                    ValidationError = "Password needs to be at least 6 characters long, with at least one uppercase letter, one lowercase letter, and one number";
                    return;
                }

                var emailRegex = new Regex(@"^\S+@\S+\.\S+$");
                if (!emailRegex.IsMatch(Email))
                {
                    ValidationError = "Incorrect email format";
                    return;
                }

                var cleanedPostalCode = PostalCode.Replace(" ", "").ToUpper();
                if (!Regex.IsMatch(cleanedPostalCode, @"^\d{4}[A-Z]{2}$"))
                {
                    ValidationError = "Postal code must be in the format 1234AB.";
                    return;
                }
                PostalCode = cleanedPostalCode;

                var client = _httpClientFactory.CreateClient("PingItClient");
                var registerDto = new RegisterDto
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    Password = Password,
                    PhoneNumber = PhoneNumber,
                    WantsNotifications = WantsNotifications,
                    Street = Street,
                    HouseNumber = HouseNumber,
                    PostalCode = PostalCode,
                    City = City
                };

                var response = await client.PostAsJsonAsync("api/auth/register", registerDto);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Registration failed: {Error}", error);
                    ValidationError = "Registration failed: " + error;
                    return;
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                if (result != null && !string.IsNullOrEmpty(result.Token))
                {
                    await _tokenStorage.StoreTokenAsync(result.Token, result.Role);
                    await Shell.Current.GoToAsync("//AccountPage");
                }
                else
                {
                    _logger.LogWarning("No token received after registration.");
                    ValidationError = "Invalid response from server.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error");
                ValidationError = "Something went wrong during registration.";
            }
            finally
            {
                IsBusy = false;
                RegisterCommand.NotifyCanExecuteChanged();
            }
        }
    }
}
