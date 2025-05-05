using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using PingIt.Shared.Dtos;
using PingIt.Maui.Services;
using PingIt.Maui.Dtos;

namespace PingIt.Maui.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RegisterViewModel> _logger;
        private readonly TokenStorageService _tokenStorage;

        public RegisterViewModel(
            TokenStorageService tokenStorage,
            IHttpClientFactory httpClientFactory,
            ILogger<RegisterViewModel> logger)
        {
            _tokenStorage = tokenStorage;
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            RegisterCommand = new Command(async () => await OnRegisterAsync(), () => IsNotBusy);
        }

        public ICommand RegisterCommand { get; }

        public string FirstName { get => firstName; set => SetProperty(ref firstName, value); }
        public string LastName { get => lastName; set => SetProperty(ref lastName, value); }
        public string Email { get => email; set => SetProperty(ref email, value); }
        public string Password { get => password; set => SetProperty(ref password, value); }
        public string ConfirmPassword { get => confirmPassword; set => SetProperty(ref confirmPassword, value); }
        public string? PhoneNumber { get => phoneNumber; set => SetProperty(ref phoneNumber, value); }
        public string Street { get => street; set => SetProperty(ref street, value); }
        public string HouseNumber { get => houseNumber; set => SetProperty(ref houseNumber, value); }
        public string PostalCode { get => postalCode; set => SetProperty(ref postalCode, value); }
        public string City { get => city; set => SetProperty(ref city, value); }
        public bool WantsNotifications { get => wantsNotifications; set => SetProperty(ref wantsNotifications, value); }
        public string ValidationError { get => validationError; set => SetProperty(ref validationError, value); }

        private string firstName = string.Empty;
        private string lastName = string.Empty;
        private string email = string.Empty;
        private string password = string.Empty;
        private string confirmPassword = string.Empty;
        private string? phoneNumber;
        private string street = string.Empty;
        private string houseNumber = string.Empty;
        private string postalCode = string.Empty;
        private string city = string.Empty;
        private bool wantsNotifications = false;
        private string validationError = string.Empty;

        private async Task OnRegisterAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ((Command)RegisterCommand).ChangeCanExecute();

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
                    ValidationError = "Password needs to be at least 6 digits long, with at least a Capital letter, small letter and number";
                    return;
                }

                var emailRegex = new Regex(@"^\S+@\S+\.\S+$");
                if (!emailRegex.IsMatch(Email))
                {
                    ValidationError = "Incorrect Email";
                    return;
                }

                var cleanedPostalCode = PostalCode.Replace(" ", "").ToUpper();
                if (!Regex.IsMatch(cleanedPostalCode, @"^\d{4}[A-Z]{2}$"))
                {
                    ValidationError = "Postcode moet in het formaat 1234AB zijn.";
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
                    ValidationError = "Registratie mislukt: " + error;
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
                    ValidationError = "Ongeldig antwoord van de server.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error");
                ValidationError = "Er ging iets mis tijdens registreren.";
            }
            finally
            {
                IsBusy = false;
                ((Command)RegisterCommand).ChangeCanExecute();
            }
        }
    }
}
