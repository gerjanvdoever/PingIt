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
        private readonly ITokenStorageService _tokenStorage;

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
            ITokenStorageService tokenStorage)
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
                    "Missing Information",
                    "Please provide a title and select a location on the map before submitting your report.",
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
                        "Submission Failed",
                        "Unable to submit your incident report. Please check your connection and try again.",
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
                        "Received incomplete data from server. Please try again.",
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
                    "Thank You!",
                    "Your incident report has been successfully submitted. Authorities have been notified and will respond accordingly.\n\nYour contribution helps keep our community safe!",
                    "Great!");

                await ClearFormAsync();
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SendAsync failed: {ex}");
                await Shell.Current.DisplayAlert(
                    "Submission Error",
                    "Something went wrong while submitting your report. Please try again later.",
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
                        "Location Permission Required",
                        "Location access is needed to mark where the incident occurred. Please enable location permissions in your device settings.",
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
                else
                {
                    await Shell.Current.DisplayAlert(
                        "Location Unavailable",
                        "Could not determine your current location. Please mark the location manually on the map.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UseCurrentLocationAsync failed: {ex}");
                await Shell.Current.DisplayAlert(
                    "Location Error",
                    "Could not retrieve your current location. Please mark the location manually on the map or try again.",
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

            try
            {
                var cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
                var photoStatus = await Permissions.RequestAsync<Permissions.Photos>();

                if (cameraStatus != PermissionStatus.Granted && photoStatus != PermissionStatus.Granted)
                {
                    await Shell.Current.DisplayAlert(
                        "Permissions Required",
                        "Camera and photo access are needed to add images to your report. Please enable these permissions in your device settings.",
                        "OK");
                    return;
                }

                var options = new List<string>();
                bool canCapture = DeviceInfo.Platform != DevicePlatform.WinUI && cameraStatus == PermissionStatus.Granted;

                if (canCapture)
                    options.Add("Take Photo");

                if (photoStatus == PermissionStatus.Granted)
                    options.Add("Choose from Gallery");

                if (options.Count == 0)
                {
                    await Shell.Current.DisplayAlert(
                        "No Options Available",
                        "Unable to access camera or photo gallery. Please check your permissions.",
                        "OK");
                    return;
                }

                var choice = await Shell.Current.DisplayActionSheet(
                    "Add Photo to Report",
                    "Cancel",
                    null,
                    options.ToArray());

                if (choice == "Cancel" || choice == null) return;

                FileResult? result = choice switch
                {
                    "Take Photo" => await MediaPicker.CapturePhotoAsync(
                                          new MediaPickerOptions { Title = $"incident_{DateTime.Now:yyyyMMdd_HHmmss}" }),
                    "Choose from Gallery" => await MediaPicker.PickPhotoAsync(
                                          new MediaPickerOptions { Title = "Select a photo for your report" }),
                    _ => null
                };

                if (result != null)
                {
                    var stream = await result.OpenReadAsync();
                    var uploadPath = await UploadLocalAsync(result.FileName, stream);

                    // Create ImageSource from the saved file path
                    var imageSource = ImageSource.FromFile(uploadPath);
                    Photos.Add(imageSource);

                    // Force UI update
                    OnPropertyChanged(nameof(Photos));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AddPhotoAsync failed: {ex}");
                await Shell.Current.DisplayAlert(
                    "Photo Error",
                    "Could not add the photo to your report. Please try again.",
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void RemovePhoto(ImageSource photo)
        {
            if (photo == null) return;

            try
            {
                // If it's a FileImageSource, try to delete the local file
                if (photo is FileImageSource fis && !string.IsNullOrEmpty(fis.File) && File.Exists(fis.File))
                {
                    try
                    {
                        File.Delete(fis.File);
                        Debug.WriteLine($"Deleted local file: {fis.File}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Could not delete local file {fis.File}: {ex.Message}");
                        // Continue anyway - the file might be in use or already deleted
                    }
                }

                Photos.Remove(photo);
                OnPropertyChanged(nameof(Photos));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RemovePhoto failed: {ex}");
            }
        }

        private Task ClearFormAsync()
        {
            Title = string.Empty;
            Description = string.Empty;
            Photos.Clear();
            NewIncidentCoord = null;
            ShowUserLocation = false;
            return Task.CompletedTask;
        }

        private async static Task<string> UploadLocalAsync(string fileName, Stream stream)
        {
            var LocalPath = Path.Combine(FileSystem.AppDataDirectory, fileName);
            using var fileStream = new FileStream(LocalPath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(fileStream);

            return LocalPath;
        }
    }
}