using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using PingIt.Maui.Services;
using PingIt.Shared.Dtos;
using CommunityToolkit.Maui.Core;

namespace PingIt.Maui.ViewModels
{
    public partial class MyIncidentDetailViewModel : ObservableObject
    {
        private readonly IIncidentStore _store;
        private readonly HttpClient _httpClient;

        [ObservableProperty]
        private IncidentDto incident = default!;

        public IEnumerable<LocationDto> PinItems => new[]
        {
            new LocationDto
            {
                Latitude  = Incident.Latitude,
                Longitude = Incident.Longitude
            }
        };

        public bool HasHandledAt => Incident.HandledAt.HasValue;

        public MyIncidentDetailViewModel(IIncidentStore store, IHttpClientFactory httpClientFactory)
        {
            _store = store;
            _httpClient = httpClientFactory.CreateClient("AuthenticatedClient");
            Incident = store.SelectedIncident ?? throw new InvalidOperationException();
            OnPropertyChanged(nameof(HasHandledAt));
            OnPropertyChanged(nameof(PinItems));
        }

        [RelayCommand]
        public async Task SaveIncidentAsync()
        {
            var updateDto = new IncidentUpdateDto
            {
                Title = Incident.Title,
                Description = Incident.Description
            };

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/incident/{Incident.Id}", updateDto);

                if (response.IsSuccessStatusCode)
                {
                    await Toast.Make("Incident opgeslagen", ToastDuration.Short).Show();
                }
                else
                {
                    await Toast.Make($"Opslaan mislukt: {response.StatusCode}", ToastDuration.Long).Show();
                }
            }
            catch (Exception ex)
            {
                await Toast.Make($"Fout bij opslaan: {ex.Message}", ToastDuration.Long).Show();
            }
        }

        [RelayCommand]
        async Task DeleteIncidentAsync()
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/incident/{Incident.Id}");

                if (response.IsSuccessStatusCode)
                {
                    await Toast.Make("Incident verwijderd", ToastDuration.Short).Show();
                    await Shell.Current.GoToAsync("//MyIncidentList");
                }
                else
                {
                    await Toast.Make($"Verwijderen mislukt: {response.StatusCode}", ToastDuration.Long).Show();
                }
            }
            catch (Exception ex)
            {
                await Toast.Make($"Fout bij verwijderen: {ex.Message}", ToastDuration.Long).Show();
            }
        }


    }
}
