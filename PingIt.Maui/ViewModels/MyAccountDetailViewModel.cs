using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PingIt.Maui.Services;
using PingIt.Shared.Dtos;

namespace PingIt.Maui.ViewModels
{
    public partial class MyAccountDetailViewModel : ObservableObject
    {
        private readonly IUserStore _userStore;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MyAccountDetailViewModel> _logger;
        private readonly TokenStorageService _tokenStorage;

        public MyAccountDetailViewModel(
            IUserStore userStore,
            IHttpClientFactory httpClientFactory,
            ILogger<MyAccountDetailViewModel> logger,
            TokenStorageService tokenStorage)
        {
            _userStore = userStore;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _tokenStorage = tokenStorage;
        }

        [ObservableProperty] private bool isLoading;

        [ObservableProperty] private string firstName = string.Empty;
        [ObservableProperty] private string lastName = string.Empty;
        [ObservableProperty] private string email = string.Empty;
        [ObservableProperty] private string phoneNumber = string.Empty;
        [ObservableProperty] private bool wantsNotifications;
        [ObservableProperty] private string street = string.Empty;
        [ObservableProperty] private string houseNumber = string.Empty;
        [ObservableProperty] private string postalCode = string.Empty;
        [ObservableProperty] private string city = string.Empty;

        [ObservableProperty] private string currentPassword = string.Empty;
        [ObservableProperty] private string newPassword = string.Empty;
        [ObservableProperty] private string confirmPassword = string.Empty;

        public async Task InitializeAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;

                var user = _userStore.CurrentUser;
                if (user != null)
                {
                    Email = user.Email ?? string.Empty;
                    FirstName = user.FirstName ?? string.Empty;
                    LastName = user.LastName ?? string.Empty;
                    PhoneNumber = user.PhoneNumber ?? string.Empty;
                    WantsNotifications = user.WantsNotifications;
                    Street = user.Street ?? string.Empty;
                    HouseNumber = user.HouseNumber ?? string.Empty;
                    PostalCode = user.PostalCode ?? string.Empty;
                    City = user.City ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing user account data");
                await Toast.Make("Failed to load account details.", ToastDuration.Long).Show();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SaveChangesAsync()
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                var validationError = ValidateUserInput();
                if (!string.IsNullOrEmpty(validationError))
                {
                    await Toast.Make(validationError, ToastDuration.Long).Show();
                    return;
                }

                var userId = _tokenStorage.UserId;
                if (userId == null)
                {
                    await Toast.Make("Session expired. Please log in again.", ToastDuration.Long).Show();
                    return;
                }

                var updateDto = new UpdateUserDto
                {
                    Email = Email.Trim(),
                    FirstName = FirstName.Trim(),
                    LastName = LastName.Trim(),
                    PhoneNumber = PhoneNumber.Trim(),
                    WantsNotifications = WantsNotifications,
                    Street = Street.Trim(),
                    HouseNumber = HouseNumber.Trim(),
                    PostalCode = PostalCode.Trim(),
                    City = City.Trim()
                };

                var client = _httpClientFactory.CreateClient("AuthenticatedClient");
                var response = await client.PutAsJsonAsync($"api/user/{userId}", updateDto);

                if (response.IsSuccessStatusCode)
                {
                    await Toast.Make("Profile updated successfully.", ToastDuration.Short).Show();
                }
                else
                {
                    await Toast.Make($"Update failed: {response.StatusCode}", ToastDuration.Long).Show();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving user data");
                await Toast.Make($"Error saving: {ex.Message}", ToastDuration.Long).Show();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ChangePasswordAsync()
        {
            if (IsLoading) return;
            IsLoading = true;

            try
            {
                var validationError = ValidatePasswordInput();
                if (!string.IsNullOrEmpty(validationError))
                {
                    await Toast.Make(validationError, ToastDuration.Long).Show();
                    return;
                }

                var userId = _tokenStorage.UserId;
                if (userId == null)
                {
                    await Toast.Make("Session expired. Please log in again.", ToastDuration.Long).Show();
                    return;
                }

                var dto = new ChangePasswordDto
                {
                    OldPassword = CurrentPassword,
                    NewPassword = NewPassword,
                    ConfirmPassword = ConfirmPassword
                };

                var client = _httpClientFactory.CreateClient("AuthenticatedClient");
                var response = await client.PutAsJsonAsync($"api/auth/change-password/{userId}", dto);

                if (response.IsSuccessStatusCode)
                {
                    await Toast.Make("Password changed successfully.", ToastDuration.Short).Show();
                    CurrentPassword = string.Empty;
                    NewPassword = string.Empty;
                    ConfirmPassword = string.Empty;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Password change failed: " + error);
                    await Toast.Make("Password change failed.", ToastDuration.Long).Show();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                await Toast.Make($"Error changing password: {ex.Message}", ToastDuration.Long).Show();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string ValidateUserInput()
        {
            if (string.IsNullOrWhiteSpace(FirstName) ||
                string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Street) ||
                string.IsNullOrWhiteSpace(HouseNumber) ||
                string.IsNullOrWhiteSpace(PostalCode) ||
                string.IsNullOrWhiteSpace(City))
            {
                return "All fields are required.";
            }

            var emailRegex = new Regex(@"^\S+@\S+\.\S+$");
            if (!emailRegex.IsMatch(Email))
            {
                return "Invalid email format.";
            }

            var postalCodeRegex = new Regex(@"^\d{4}[A-Z]{2}$");
            if (!postalCodeRegex.IsMatch(PostalCode))
            {
                return "Postal code must be in the format 1234AB.";
            }

            return string.Empty;
        }

        private string ValidatePasswordInput()
        {
            if (string.IsNullOrWhiteSpace(CurrentPassword) ||
                string.IsNullOrWhiteSpace(NewPassword) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                return "All password fields are required.";
            }

            if (NewPassword != ConfirmPassword)
            {
                return "New password and confirmation do not match.";
            }

            var passwordRegex = new Regex(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9]).{8,}$");
            if (!passwordRegex.IsMatch(NewPassword))
            {
                return "Password must be at least 8 characters long and include an uppercase letter, a lowercase letter, and a number.";
            }

            return string.Empty;
        }
    }
}
