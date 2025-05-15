using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;
using PingIt.Shared.Dtos;

namespace PingIt.Maui.ViewModels
{
    public partial class ReportIncidentViewModel : ObservableObject
    {
        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        public ObservableCollection<ImageSource> Photos { get; } = new();

        [ObservableProperty]
        private LocationDto? newIncidentCoord;

        [ObservableProperty]
        private bool showUserLocation;

        [ObservableProperty]
        private bool isBusy;

        public bool IsNotBusy => !IsBusy;

        public ReportIncidentViewModel() { }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task UseCurrentLocationAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // 1. Ask for Location permission
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlert(
                        "Permission Denied",
                        "Locatietoegang is vereist om uw positie te bepalen.",
                        "OK");
                    return;
                }

                // 2. Get last known or fresh location
                var result = await Geolocation.GetLastKnownLocationAsync()
                           ?? await Geolocation.GetLocationAsync(
                                 new GeolocationRequest(
                                   GeolocationAccuracy.Medium,
                                   TimeSpan.FromSeconds(10)));

                if (result != null)
                {
                    NewIncidentCoord = new LocationDto
                    {
                        Latitude = (decimal)result.Latitude,
                        Longitude = (decimal)result.Longitude
                    };
                    // show the blue-dot too
                    ShowUserLocation = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UseCurrentLocationAsync failed: {ex}");
                await Shell.Current.DisplayAlert(
                    "Fout",
                    "Kon huidige locatie niet ophalen.",
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private async Task AddPhotoAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            var status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                await Shell.Current.DisplayAlert("Permissie geweigerd", "Camera toegang is vereist.", "OK");
                return;
            }

            try
            {
                var options = new List<string>();
                bool canCapture = DeviceInfo.Platform != DevicePlatform.WinUI;
                if (canCapture)
                    options.Add("Take Photo");
                options.Add("Choose Photo");

                var choice = await Shell.Current.DisplayActionSheet(
                    "Foto toevoegen",
                    "Annuleer",
                    null,
                    options.ToArray());

                if (choice == "Annuleer") return;

                FileResult? result = choice switch
                {
                    "Take Photo" => await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
                    {
                        Title = $"incident_{DateTime.Now:yyyyMMdd_HHmmss}"
                    }),
                    "Choose Photo" => await MediaPicker.PickPhotoAsync(new MediaPickerOptions
                    {
                        Title = "Select a photo"
                    }),
                    _ => null
                };

                if (result != null)
                {
                    string savedPath = await SavePhotoAsync(result);
                    Photos.Add(ImageSource.FromFile(savedPath));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AddPhotoAsync failed: {ex}");
                await Shell.Current.DisplayAlert("Fout", "Kon de foto niet toevoegen.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task<string> SavePhotoAsync(FileResult photo)
        {
            var localFilePath = Path.Combine(
                FileSystem.CacheDirectory,
                photo.FileName);

            using Stream sourceStream = await photo.OpenReadAsync();
            using FileStream destStream = File.OpenWrite(localFilePath);

            await sourceStream.CopyToAsync(destStream);

            return localFilePath;
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private void Send()
        {
            // stub for future implementation
        }
    }
}
