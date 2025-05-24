using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using PingIt.Maui.Services;
using PingIt.Maui.Views;
using PingIt.Shared.Dtos;
using PingIt.Shared.Enums;

namespace PingIt.Maui.ViewModels
{
    public partial class AccountViewModel : ObservableObject
    {
        // … your injected services …
        private readonly TokenStorageService _tokenStorage;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AccountViewModel> _logger;
        private readonly IIncidentStore _store;

        // your observable props
        [ObservableProperty] string firstName = string.Empty;
        [ObservableProperty] string lastName = string.Empty;
        [ObservableProperty] bool isBusy;
        [ObservableProperty] bool isLoading;
        [ObservableProperty] bool isFooterVisible;
        [ObservableProperty] bool isDropdownVisible;
        [ObservableProperty] ObservableCollection<IncidentDto> incidents = new();
        [ObservableProperty] IncidentDto? selectedIncident;

        public string FullName
        {
            get
            {
                var hour = DateTime.Now.Hour;
                var greeting = hour switch
                {
                    >= 5 and < 12 => "Good Morning",
                    >= 12 and < 17 => "Good Afternoon",
                    _ => "Good Evening"
                };
                return $"{greeting}, {FirstName}";
            }
        }

        public int ActiveIncidentsCount
        {
            get
            {
                return Incidents?.Count(i => i.Status == IncidentStatus.Reported ||
                                           i.Status == IncidentStatus.Registered ||
                                           i.Status == IncidentStatus.InProgress) ?? 0;
            }
        }

        public AccountViewModel(
            TokenStorageService tokenStorage,
            IHttpClientFactory httpClientFactory,
            ILogger<AccountViewModel> logger,
            IIncidentStore store)
        {
            _tokenStorage = tokenStorage;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _store = store;
        }

        public async Task InitializeAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                await LoadUserAsync();
                await LoadIncidentsAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ——— COMMANDS ———

        [RelayCommand]
        private void ToggleDropdown()
            => IsDropdownVisible = !IsDropdownVisible;

        [RelayCommand]
        private void CloseDropdown()
            => IsDropdownVisible = false;

        [RelayCommand]
        private void Logout()
        {
            CloseDropdown();
            _tokenStorage
                .ClearTokenAsync()
                .ContinueWith(_ =>
                    MainThread.BeginInvokeOnMainThread(async () =>
                        await Shell.Current.GoToAsync("//LoginPage")));
        }

        [RelayCommand]
        private async Task LoadIncidentsAsync()
        {
            if (_tokenStorage.UserId == null) return;

            try
            {
                IsLoading = true;
                var client = _httpClientFactory.CreateClient("AuthenticatedClient");
                var resp = await client.GetAsync($"api/incident/user/{_tokenStorage.UserId}");
                if (!resp.IsSuccessStatusCode) return;

                var list = await resp
                    .Content
                    .ReadFromJsonAsync<IncidentDto[]>()
                           ?? Array.Empty<IncidentDto>();
                Incidents = new ObservableCollection<IncidentDto>(list);

                // Notify that ActiveIncidentsCount has changed
                OnPropertyChanged(nameof(ActiveIncidentsCount));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading incidents");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task NewIncidentAsync()
        {
            IsBusy = true;
            try
            {
                await Shell.Current.GoToAsync(nameof(ReportIncidentPage));
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ——— end COMMANDS ———

        private async Task LoadUserAsync()
        {
            var userId = _tokenStorage.UserId;
            if (userId == null)
            {
                LogoutCommand.Execute(null);
                return;
            }

            var client = _httpClientFactory.CreateClient("AuthenticatedClient");
            var resp = await client.GetAsync($"api/user/{userId}");
            if (!resp.IsSuccessStatusCode)
            {
                LogoutCommand.Execute(null);
                return;
            }

            var user = await resp.Content.ReadFromJsonAsync<UserDto>();
            if (user != null)
            {
                FirstName = user.FirstName;
                LastName = user.LastName;
                OnPropertyChanged(nameof(FullName));
                IsFooterVisible = user.Role is UserRole.Administrator or UserRole.Worker;
            }
        }

        partial void OnSelectedIncidentChanged(IncidentDto? dto)
            => _ = HandleSelection(dto);

        private async Task HandleSelection(IncidentDto? dto)
        {
            if (dto == null) return;
            IsLoading = true;
            _store.SelectedIncident = dto;
            await Shell.Current.GoToAsync(nameof(MyIncidentDetail));
            SelectedIncident = null;
            IsLoading = false;
        }

        // Override the Incidents property setter to notify ActiveIncidentsCount changes
        partial void OnIncidentsChanged(ObservableCollection<IncidentDto> value)
        {
            OnPropertyChanged(nameof(ActiveIncidentsCount));
        }
    }
}