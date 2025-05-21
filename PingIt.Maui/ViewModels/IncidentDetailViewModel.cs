using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using PingIt.Maui.Services;
using PingIt.Shared.Dtos;
using PingIt.Shared.Enums;
using CommunityToolkit.Maui.Core;

namespace PingIt.Maui.ViewModels
{
    public partial class IncidentDetailViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;
        private readonly IIncidentStore _store;

        [ObservableProperty]
        private IncidentDto incident = default!;

        [ObservableProperty]
        private IncidentStatus selectedStatus;

        [ObservableProperty]
        private bool isBusy;
        public bool IsNotBusy => !IsBusy;

        public List<IncidentStatus> StatusOptions { get; } =
            Enum.GetValues<IncidentStatus>().Cast<IncidentStatus>().ToList();

        public IEnumerable<LocationDto> PinItems =>
            new[]
            {
                new LocationDto
                {
                    Latitude  = Incident.Latitude,
                    Longitude = Incident.Longitude
                }
            };

        public IncidentDetailViewModel(
            IIncidentStore store,
            IHttpClientFactory httpClientFactory)
        {
            _store = store;
            _httpClient = httpClientFactory.CreateClient("AuthenticatedClient");

            Incident = store.SelectedIncident
                       ?? throw new InvalidOperationException("No incident selected");

            SelectedStatus = Incident.Status;

            OnPropertyChanged(nameof(PinItems));
        }

        [RelayCommand]
        public async Task ChangeStatusAsync()
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                var dto = new IncidentStatusUpdateDto
                {
                    NewStatus = SelectedStatus,
                    Notes = Incident.Notes
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"api/incidents/{Incident.Id}/status", dto);

                if (response.IsSuccessStatusCode)
                {
                    Incident.Status = SelectedStatus;
                    OnPropertyChanged(nameof(Incident));

                    await Toast.Make("Status and notes saved", ToastDuration.Short).Show();
                }
                else
                {
                    await Toast.Make(
                        $"Update failed: {response.StatusCode}",
                        ToastDuration.Long).Show();
                }
            }
            catch (Exception ex)
            {
                await Toast.Make(
                    $"Error: {ex.Message}",
                    ToastDuration.Long).Show();
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
