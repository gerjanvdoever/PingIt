using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace PingIt.Maui.ViewModels
{
    public partial class ReportIncidentViewModel : ObservableObject
    {
        // --- Fields & properties ---

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string description = string.Empty;

        // Holds the image sources for display; will be populated via AddPhotoCommand
        public ObservableCollection<ImageSource> Photos { get; } = new();

        [ObservableProperty]
        private bool isBusy;

        // Helper for button enablement
        public bool IsNotBusy => !IsBusy;

        // --- Constructor ---

        public ReportIncidentViewModel()
        {
        }

        // --- Commands ---

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private async Task AddPhotoAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                // TODO: launch Camera or FilePicker to pick/take a photo
                // var fileResult = await MediaPicker.Default.PickPhotoAsync();
                // if (fileResult != null)
                // {
                //     var stream = await fileResult.OpenReadAsync();
                //     Photos.Add(ImageSource.FromStream(() => stream));
                // }
            }
            catch (Exception ex)
            {
                // TODO: handle any errors (permissions, cancellation, etc.)
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(IsNotBusy))]
        private void Send()
        {
            // TODO: implement API call to POST IncidentDto
            // right now this is a stub for future work
        }
    }
}
