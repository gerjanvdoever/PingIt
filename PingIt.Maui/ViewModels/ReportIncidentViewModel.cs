using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;

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
        private bool isBusy;

        public bool IsNotBusy => !IsBusy;

        public ReportIncidentViewModel() { }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private async Task AddPhotoAsync()
        {
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                // Build action-sheet choices
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

                if (choice == "Cancel")
                    return;

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
                    // 1) Open the file stream
                    using var sourceStream = await result.OpenReadAsync();

                    // 2) Copy into a MemoryStream that lives beyond this block
                    var memoryStream = new MemoryStream();
                    await sourceStream.CopyToAsync(memoryStream);

                    // 3) IMPORTANT: reset so reads start at the beginning
                    memoryStream.Position = 0;

                    // 4) Create an ImageSource that re-uses (or re-clones) that MemoryStream
                    Photos.Add(ImageSource.FromStream(() =>
                    {
                        // rewind before each read, in case the UI reads multiple times
                        memoryStream.Position = 0;
                        return memoryStream;
                    }));
                }
            }
            catch (FeatureNotSupportedException)
            {
                await Shell.Current.DisplayAlert(
                    "Not supported",
                    "Camera is not available on this device.",
                    "OK");
            }
            catch (PermissionException)
            {
                await Shell.Current.DisplayAlert(
                    "Permission denied",
                    "Please grant permission to use the camera or storage.",
                    "OK");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"AddPhotoAsync failed: {ex}");
                await Shell.Current.DisplayAlert(
                    "Error",
                    "Something went wrong while adding the photo.",
                    "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }


        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private void Send()
        {
            // stub for future implementation
        }
    }
}
