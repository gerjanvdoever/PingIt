using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PingIt.Shared.Dtos;
using PingIt.Maui.Services;
using System.Net.Http.Json;
using System.Collections.ObjectModel;

namespace PingIt.Maui.ViewModels;

public partial class IncidentListViewModel : ObservableObject
{
    private readonly TokenStorageService _tokenStorage;
    private readonly HttpClient _httpClient;
    private readonly ILogger<IncidentListViewModel> _logger;

    [ObservableProperty] private ObservableCollection<IncidentDto> incidents = new();
    [ObservableProperty] private ObservableCollection<IncidentDto> closedIncidents = new();
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private bool showClosed;

    public IncidentListViewModel(
        TokenStorageService tokenStorage,
        IHttpClientFactory httpClientFactory,
        ILogger<IncidentListViewModel> logger)
    {
        _tokenStorage = tokenStorage;
        _httpClient = httpClientFactory.CreateClient("AuthenticatedClient");
        _logger = logger;
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
            var response = await _httpClient.GetAsync($"api/incident/worker/{userId}/active");

            if (!response.IsSuccessStatusCode)
            {
                StatusMessage = "Kan incidenten niet ophalen.";
                _logger.LogWarning("Failed to fetch incidents: {StatusCode}", response.StatusCode);
                return;
            }

            var result = await response.Content.ReadFromJsonAsync<List<IncidentDto>>();
            Incidents = new ObservableCollection<IncidentDto>(result ?? []);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij ophalen incidenten");
            StatusMessage = "Er ging iets mis bij het ophalen van incidenten.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadClosedIncidentsAsync()
    {
        if (_tokenStorage.UserId is null)
        {
            StatusMessage = "Gebruiker niet ingelogd.";
            return;
        }

        try
        {
            ShowClosed = true;
            IsLoading = true;
            StatusMessage = string.Empty;

            int userId = _tokenStorage.UserId.Value;
            var response = await _httpClient.GetAsync($"api/incident/worker/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                StatusMessage = "Kan afgesloten incidenten niet ophalen.";
                _logger.LogWarning("Failed to fetch closed incidents: {StatusCode}", response.StatusCode);
                return;
            }

            var result = await response.Content.ReadFromJsonAsync<List<IncidentDto>>();
            closedIncidents = new ObservableCollection<IncidentDto>(result ?? []);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fout bij ophalen afgesloten incidenten");
            StatusMessage = "Er ging iets mis bij het ophalen van afgesloten incidenten.";
        }
        finally
        {
            IsLoading = false;
        }
    }


}
