using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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
        private readonly TokenStorageService _tokenStorage;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AccountViewModel> _logger;
        private readonly IIncidentStore _store;

        [ObservableProperty] private string firstName = string.Empty;
        [ObservableProperty] private string lastName = string.Empty;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private bool isFooterVisible;
        [ObservableProperty] private ObservableCollection<IncidentDto> incidents = new();
        [ObservableProperty] private IncidentDto? selectedIncident;

        public IAsyncRelayCommand LoadIncidentsCommand { get; }
        public IRelayCommand LogoutCommand { get; }
        public IAsyncRelayCommand NewIncidentCommand { get; }

        partial void OnSelectedIncidentChanged(IncidentDto? dto)
            => _ = HandleSelection(dto);

        public string FullName
        {
            get
            {
                var hour = DateTime.Now.Hour;
                string greeting;

                if (hour >= 5 && hour < 12)
                    greeting = "Good Morning";
                else if (hour >= 12 && hour < 17)
                    greeting = "Good Afternoon";
                else
                    greeting = "Good Evening";

                return $"{greeting}, {FirstName}";
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

            LoadIncidentsCommand = new AsyncRelayCommand(LoadIncidentsAsync);
            LogoutCommand = new RelayCommand(OnLogout);
            NewIncidentCommand = new AsyncRelayCommand(OnNewIncident);
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

        private async Task LoadUserAsync()
        {
            var userId = _tokenStorage.UserId;
            if (userId == null)
            {
                OnLogout();
                return;
            }

            var client = _httpClientFactory.CreateClient("AuthenticatedClient");
            var resp = await client.GetAsync($"api/user/{userId}");
            if (!resp.IsSuccessStatusCode)
            {
                OnLogout();
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

        private async Task LoadIncidentsAsync()
        {
            if (_tokenStorage.UserId == null) return;

            try
            {
                IsLoading = true;
                var client = _httpClientFactory.CreateClient("AuthenticatedClient");
                var response = await client.GetAsync($"api/incident/user/{_tokenStorage.UserId}");
                if (!response.IsSuccessStatusCode) return;

                var list = await response.Content.ReadFromJsonAsync<IncidentDto[]>() ?? Array.Empty<IncidentDto>();
                Incidents = new ObservableCollection<IncidentDto>(list);
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

        private async Task HandleSelection(IncidentDto? dto)
        {
            if (dto == null) return;
            IsLoading = true;
            _store.SelectedIncident = dto;
            await Shell.Current.GoToAsync(nameof(MyIncidentDetail));
            SelectedIncident = null;
            IsLoading = false;
        }

        private void OnLogout()
            => _tokenStorage
                .ClearTokenAsync()
                .ContinueWith(_ =>
                    MainThread.BeginInvokeOnMainThread(async () =>
                        await Shell.Current.GoToAsync("//LoginPage")));

        private async Task OnNewIncident()
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

    }
}
