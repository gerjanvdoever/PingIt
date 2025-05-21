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
                    "Error",
                    "Please fill in the necessary information.",
                    "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var incidentDto = new IncidentDto
                {
                    Title = Title,
                    Description = Description,
                    Latitude = NewIncidentCoord.Latitude,
                    Longitude = NewIncidentCoord.Longitude,
                    CreatedByUserId = _tokenStorage.UserId
                };

                var incidentResp = await _httpClient
                    .PostAsJsonAsync("api/incident", incidentDto);

                if (!incidentResp.IsSuccessStatusCode)
                {
                    await Shell.Current.DisplayAlert(
                        "Error",
                        "Unable to create incident.",
                        "OK");
                    return;
                }

                var created = await incidentResp
                    .Content
                    .ReadFromJsonAsync<IncidentDto>();

                if (created?.Id == null)
                {
                    await Shell.Current.DisplayAlert(
                        "Error",
                        "Received incomplete data from server.",
                        "OK");
                    return;
                }

                int incidentId = created.Id;

                foreach (var img in Photos)
                {
                    if (img is FileImageSource fis && File.Exists(fis.File))
                    {
                        byte[] bytes = File.ReadAllBytes(fis.File);
                        var base64 = Convert.ToBase64String(bytes);

                        var photoDto = new IncidentPhotoDto { PhotoUrl = base64 };

                        var photoResp = await _httpClient
                            .PostAsJsonAsync(
                                $"api/incident/{incidentId}/photos",
                                photoDto);

                        if (!photoResp.IsSuccessStatusCode)
                        {
                            Debug.WriteLine(
                              $"Photo upload failed for {fis.File}: {photoResp.StatusCode}");
                        }
                    }
                }

                await Shell.Current.DisplayAlert(
                    "Done",
                    "Incident sent successfully!",
                    "OK");

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SendAsync failed: {ex}");
                await Shell.Current.DisplayAlert(
                    "Error",
                    "Something went wrong while sending.",
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
                        "Location access is required to determine your position.",
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
                    "Error",
                    "Could not retrieve current location.",
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
                await Shell.Current.DisplayAlert(
                    "Permission Denied",
                    "Camera access is required.",
                    "OK");
                return;
            }

            var photoStatus = await Permissions.RequestAsync<Permissions.Photos>();
            if (photoStatus != PermissionStatus.Granted)
            {
                await Shell.Current.DisplayAlert(
                    "Permission Denied",
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
                    "Add Photo",
                    "Cancel",
                    null,
                    options.ToArray());

                if (choice == "Cancel") return;

                FileResult? result = choice switch
                {
                    "Take Photo" => await MediaPicker.CapturePhotoAsync(
                                          new MediaPickerOptions { Title = $"incident_{DateTime.Now:yyyyMMdd_HHmmss}" }),
                    "Choose Photo" => await MediaPicker.PickPhotoAsync(
                                          new MediaPickerOptions { Title = "Select a photo" }),
                    _ => null
                };

                if (result != null)
                {
                    var stream = await result.OpenReadAsync();
                    var uploadPath = await UploadLocalAsync(result.FileName, stream);
                    var imageSource = ImageSource.FromFile(uploadPath);
                    Photos.Add(imageSource);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AddPhotoAsync failed: {ex}");
                await Shell.Current.DisplayAlert(
                    "Error",
                    "Could not add the photo.",
                    "OK");
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
