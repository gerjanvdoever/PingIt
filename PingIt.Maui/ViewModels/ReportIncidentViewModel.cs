using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using PingIt.Maui.Services;
using PingIt.Shared.Dtos;

namespace PingIt.Maui.ViewModels
{
    public partial class ReportIncidentViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;
        private readonly TokenStorageService _tokenStorage;

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

        public ReportIncidentViewModel(
            IHttpClientFactory httpFactory,
            TokenStorageService tokenStorage)
        {
            _httpClient = httpFactory.CreateClient("AuthenticatedClient");
            _tokenStorage = tokenStorage;
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task SendAsync()
        {
            if (IsBusy) return;
            if (string.IsNullOrWhiteSpace(Title)
             || NewIncidentCoord is null)
            {
                await Shell.Current.DisplayAlert(
                    "Oeps",
                    "Zorg dat je ten minste een titel en locatie hebt ingevoerd.",
                    "OK");
                return;
            }

            IsBusy = true;
            try
            {
                // 1) Build the payload
                var incidentDto = new IncidentDto
                {
                    Title = Title,
                    Description = Description,
                    Latitude = NewIncidentCoord.Latitude,
                    Longitude = NewIncidentCoord.Longitude,
                    // only send user ID when we have one
                    CreatedByUserId = _tokenStorage.UserId
                };

                // 2) POST the incident
                var incidentResp = await _httpClient
                    .PostAsJsonAsync("api/incident", incidentDto);

                if (!incidentResp.IsSuccessStatusCode)
                {
                    await Shell.Current.DisplayAlert(
                        "Fout",
                        "Kon incident niet aanmaken.",
                        "OK");
                    return;
                }

                // 3) Read back the created incident (to get its Id)
                var created = await incidentResp
                    .Content
                    .ReadFromJsonAsync<IncidentDto>();

                if (created?.Id == null)
                {
                    await Shell.Current.DisplayAlert(
                        "Fout",
                        "Ontvangen onvolledige data van server.",
                        "OK");
                    return;
                }

                int incidentId = created.Id;

                // 4) Upload each photo
                foreach (var img in Photos)
                {
                    if (img is FileImageSource fis && File.Exists(fis.File))
                    {
                        byte[] bytes = File.ReadAllBytes(fis.File);
                        var base64 = Convert.ToBase64String(bytes);

                        var photoDto = new IncidentPhotoDto
                        {
                            PhotoUrl = base64
                        };

                        var photoResp = await _httpClient
                            .PostAsJsonAsync(
                                $"api/incident/{incidentId}/photos",
                                photoDto);

                        if (!photoResp.IsSuccessStatusCode)
                        {
                            Debug.WriteLine(
                              $"Photo upload failed for {fis.File}: {photoResp.StatusCode}");
                            // optionally show a warning, but continue
                        }
                    }
                }

                // 5) Success!
                await Shell.Current.DisplayAlert(
                    "Klaar",
                    "Incident succesvol verstuurd!",
                    "OK");

                // go back one page
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SendAsync failed: {ex}");
                await Shell.Current.DisplayAlert(
                    "Fout",
                    "Er ging iets mis bij het versturen.",
                    "OK");
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(IsNotBusy));
            }
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        public async Task UseCurrentLocationAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlert(
                        "Permission Denied",
                        "Locatietoegang is vereist om uw positie te bepalen.",
                        "OK");
                    return;
                }

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

            var photoStatus = await Permissions.RequestAsync<Permissions.Photos>();
            if (photoStatus != PermissionStatus.Granted)
            {
                await Shell.Current.DisplayAlert("Permission Denied",
                                                  "Gallery access is required to choose a photo.",
                                                  "OK");
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
                    var stream = await result.OpenReadAsync();
                    var UploadPath = await UploadLocalAsync(result.FileName, stream);
                    var imageSource = ImageSource.FromFile(UploadPath);
                    Photos.Add(imageSource);
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

        private async Task<string> UploadLocalAsync(string fileName, Stream stream)
        {
            var LocalPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            using var fileStream = new FileStream(LocalPath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fileStream);

            return LocalPath;
        }
    }
}
