using System;
using System.Collections.Generic;
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

namespace PingIt.Maui.ViewModels;
public partial class IncidentListViewModel : ObservableObject
{
    private readonly ITokenStorageService _tokenStorage;
    private readonly HttpClient _httpClient;
    private readonly ILogger<IncidentListViewModel> _logger;
    private readonly IIncidentStore _store;

    [ObservableProperty] private ObservableCollection<IncidentDto> incidents = new();
    [ObservableProperty] private ObservableCollection<IncidentDto> closedIncidents = new();
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private bool showClosed;
    [ObservableProperty] private IncidentDto? selectedIncident;

    public IncidentListViewModel(
        ITokenStorageService tokenStorage,
        IHttpClientFactory httpClientFactory,
        ILogger<IncidentListViewModel> logger,
        IIncidentStore store)
    {
        _tokenStorage = tokenStorage;
        _httpClient = httpClientFactory.CreateClient("AuthenticatedClient");
        _logger = logger;
        _store = store;
    }

    [RelayCommand]
    private async Task NavigateToDetailAsync(IncidentDto incident)
    {
        if (incident is null)
            return;

        try
        {
            _store.SelectedIncident = incident;

            var targetPage = DeviceInfo.Platform == DevicePlatform.WinUI
                ? nameof(IncidentDetailWindowsPage)
                : nameof(IncidentDetailPage);

            await Shell.Current.GoToAsync(targetPage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to detail page");
        }
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        if (IsRefreshing)
            return;

        try
        {
            IsRefreshing = true;
            await LoadIncidentsAsync();

            // If closed incidents are currently shown, refresh them too
            if (ShowClosed)
            {
                await LoadClosedIncidentsInternalAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during refresh");
            StatusMessage = "Something went wrong while refreshing.";
        }
        finally
        {
            IsRefreshing = false;
        }
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
            if (!IsRefreshing)
                IsLoading = true;

            StatusMessage = string.Empty;

            int userId = _tokenStorage.UserId.Value;
            var response = await _httpClient.GetAsync($"api/incident/worker/{userId}/active");

            if (!response.IsSuccessStatusCode)
            {
                StatusMessage = "Couldn't retrieve incidents.";
                _logger.LogWarning("Failed to fetch incidents: {StatusCode}", response.StatusCode);
                return;
            }

            var result = await response.Content.ReadFromJsonAsync<List<IncidentDto>>();
            Incidents = new ObservableCollection<IncidentDto>(result ?? new List<IncidentDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching active incidents");
            StatusMessage = "Something went wrong while fetching your incidents.";
        }
        finally
        {
            if (!IsRefreshing)
                IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task LoadClosedIncidentsAsync()
    {
        if (ShowClosed)
        {
            ShowClosed = false;
            ClosedIncidents.Clear();
            return;
        }

        if (_tokenStorage.UserId is null)
        {
            StatusMessage = "User not logged in.";
            return;
        }

        try
        {
            IsLoading = true;
            StatusMessage = string.Empty;
            ShowClosed = true;

            await LoadClosedIncidentsInternalAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching closed incidents");
            StatusMessage = "Something went wrong while fetching closed incidents.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadClosedIncidentsInternalAsync()
    {
        if (_tokenStorage.UserId is null)
        {
            StatusMessage = "User not logged in.";
            return;
        }

        try
        {
            int userId = _tokenStorage.UserId.Value;
            var response = await _httpClient.GetAsync($"api/incident/worker/{userId}/closed");

            if (!response.IsSuccessStatusCode)
            {
                StatusMessage = "Unable to fetch closed incidents.";
                _logger.LogWarning("Failed to fetch closed incidents: {StatusCode}", response.StatusCode);
                return;
            }

            var result = await response.Content.ReadFromJsonAsync<List<IncidentDto>>();
            ClosedIncidents = new ObservableCollection<IncidentDto>(result ?? new List<IncidentDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching closed incidents internally");
            throw;
        }
    }
}