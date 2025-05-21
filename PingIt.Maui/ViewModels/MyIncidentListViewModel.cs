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

namespace PingIt.Maui.ViewModels
{
    public partial class MyIncidentListViewModel : ObservableObject
    {
        private readonly IIncidentStore _store;
        private readonly TokenStorageService _tokenStorage;
        private readonly HttpClient _httpClient;
        private readonly ILogger<MyIncidentListViewModel> _logger;

        [ObservableProperty]
        private ObservableCollection<IncidentDto> incidents = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        public MyIncidentListViewModel(
            TokenStorageService tokenStorage,
            IHttpClientFactory httpClientFactory,
            ILogger<MyIncidentListViewModel> logger,
            IIncidentStore store)
        {
            _tokenStorage = tokenStorage;
            _httpClient = httpClientFactory.CreateClient("AuthenticatedClient");
            _logger = logger;
            _store = store;
        }

        [RelayCommand]
        public async Task LoadIncidentsAsync()
        {
            if (_tokenStorage.UserId is null)
            {
                StatusMessage = "User not logged in.";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = string.Empty;

                int userId = _tokenStorage.UserId.Value;
                var resp = await _httpClient.GetAsync($"api/incident/user/{userId}");
                if (!resp.IsSuccessStatusCode)
                {
                    StatusMessage = "Unable to fetch incidents.";
                    _logger.LogWarning("LoadMyIncidents failed: {StatusCode}", resp.StatusCode);
                    return;
                }

                var list = await resp.Content.ReadFromJsonAsync<List<IncidentDto>>();
                Incidents = new ObservableCollection<IncidentDto>(list ?? new());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user's incidents");
                StatusMessage = "Something went wrong while fetching incidents.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [ObservableProperty]
        private IncidentDto? selectedIncident;

        partial void OnSelectedIncidentChanged(IncidentDto? incident)
            => HandleSelectionChangedAsync(incident);

        private async Task HandleSelectionChangedAsync(IncidentDto? incident)
        {
            if (incident is null)
                return;

            _store.SelectedIncident = incident;
            await Shell.Current.GoToAsync(nameof(MyIncidentDetail));
            SelectedIncident = null;
        }
    }
}
