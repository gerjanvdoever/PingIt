using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PingIt.Shared.Dtos;
using PingIt.Maui.Services;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using PingIt.Maui.Views;

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
                StatusMessage = "Gebruiker niet ingelogd.";
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
                    StatusMessage = "Kan incidenten niet ophalen.";
                    _logger.LogWarning("LoadMyIncidents failed: {StatusCode}", resp.StatusCode);
                    return;
                }

                var list = await resp.Content.ReadFromJsonAsync<List<IncidentDto>>();
                Incidents = new ObservableCollection<IncidentDto>(list ?? new());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen eigen incidenten");
                StatusMessage = "Er ging iets mis bij het ophalen van incidenten.";
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
