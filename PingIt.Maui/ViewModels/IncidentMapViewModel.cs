using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PingIt.Maui.Services;
using PingIt.Shared.Dtos;

namespace PingIt.Maui.ViewModels
{
    public partial class IncidentMapViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;
        private readonly TokenStorageService _tokenStorage;
        private readonly ILogger<IncidentMapViewModel> _logger;

        [ObservableProperty]
        private ObservableCollection<LocationDto> pinItems = new();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private LocationDto? selectedLocation;

        public IncidentMapViewModel(
            IHttpClientFactory factory,
            TokenStorageService tokenStorage,
            ILogger<IncidentMapViewModel> logger)
        {
            _httpClient = factory.CreateClient("AuthenticatedClient");
            _tokenStorage = tokenStorage;
            _logger = logger;

            _ = LoadIncidentsAsync();
        }

        [RelayCommand]
        public async Task LoadIncidentsAsync()
        {
            if (_tokenStorage.UserId is null)
            {
                StatusMessage = "User is not logged in.";
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = string.Empty;

                int userId = _tokenStorage.UserId.Value;
                var response = await _httpClient
                    .GetAsync($"api/incident/worker/{userId}/active");

                if (!response.IsSuccessStatusCode)
                {
                    StatusMessage = "Unable to retrieve incidents.";
                    _logger.LogWarning(
                        "Failed to fetch incidents: {StatusCode}",
                        response.StatusCode);
                    return;
                }

                var incidents = await response.Content
                    .ReadFromJsonAsync<List<IncidentDto>>()
                    ?? new List<IncidentDto>();

                PinItems = new ObservableCollection<LocationDto>(
                    incidents.Select(inc => new LocationDto
                    {
                        Latitude = inc.Latitude,
                        Longitude = inc.Longitude,
                        Label = inc.Title
                    }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching incidents");
                StatusMessage = "An error occurred while fetching incidents.";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
