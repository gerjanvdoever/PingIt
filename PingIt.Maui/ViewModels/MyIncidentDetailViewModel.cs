using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.Controls;
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
        private bool isBusy;

        [ObservableProperty]
        private IncidentDto incident = default!;

        [ObservableProperty]
        private bool isImageFullscreen;

        [ObservableProperty]
        private string fullscreenImageUrl = string.Empty;

        public bool IsNotBusy => !IsBusy;

        public IEnumerable<LocationDto> PinItems => new[]
        {
            new LocationDto
            {
                Latitude  = Incident.Latitude,
                Longitude = Incident.Longitude
            }
        };

        public bool HasHandledAt => Incident?.HandledAt.HasValue ?? false;

        public MyIncidentDetailViewModel(IIncidentStore store, IHttpClientFactory httpClientFactory)
        {
            _store = store;
            _httpClient = httpClientFactory.CreateClient("AuthenticatedClient");
            Incident = store.SelectedIncident ?? throw new InvalidOperationException();

            // Notify property changes for computed properties
            OnPropertyChanged(nameof(HasHandledAt));
            OnPropertyChanged(nameof(PinItems));
        }

        [RelayCommand]
        public async Task SaveIncidentAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                var updateDto = new IncidentUpdateDto
                {
                    Title = Incident.Title,
                    Description = Incident.Description
                };

                var response = await _httpClient.PutAsJsonAsync($"api/incident/{Incident.Id}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    await Toast.Make("Incident saved", ToastDuration.Short).Show();
                }
                else
                {
                    await Toast.Make($"Save failed: {response.StatusCode}", ToastDuration.Long).Show();
                }
            }
            catch (Exception ex)
            {
                await Toast.Make($"Error saving: {ex.Message}", ToastDuration.Long).Show();
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        public async Task OpenInGoogleMapsAsync()
        {
            if (Incident is null)
                return;

            var url = $"https://www.google.com/maps?q={Incident.Latitude},{Incident.Longitude}";
            try
            {
                await Launcher.Default.OpenAsync(new Uri(url));
            }
            catch (Exception ex)
            {
                await Toast.Make($"Failed to open map: {ex.Message}", ToastDuration.Long).Show();
            }
        }

        [RelayCommand]
        async Task DeleteIncidentAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                var response = await _httpClient.DeleteAsync($"api/incident/{Incident.Id}");
                if (response.IsSuccessStatusCode)
                {
                    await Toast.Make("Incident deleted", ToastDuration.Short).Show();
                    await Shell.Current.GoToAsync("//AccountPage");
                }
                else
                {
                    await Toast.Make($"Delete failed: {response.StatusCode}", ToastDuration.Long).Show();
                }
            }
            catch (Exception ex)
            {
                await Toast.Make($"Error deleting: {ex.Message}", ToastDuration.Long).Show();
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        void ShowFullscreenImage(string imageUrl)
        {
            FullscreenImageUrl = imageUrl;
            IsImageFullscreen = true;
        }

        [RelayCommand]
        void CloseFullscreenImage()
        {
            IsImageFullscreen = false;
            FullscreenImageUrl = string.Empty;
        }

        // Override OnPropertyChanged to handle computed properties
        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            // Update computed properties when Incident changes
            if (e.PropertyName == nameof(Incident))
            {
                OnPropertyChanged(nameof(HasHandledAt));
                OnPropertyChanged(nameof(PinItems));
            }
        }
    }
}