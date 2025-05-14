using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PingIt.Maui.Services;
using PingIt.Shared.Dtos;

namespace PingIt.Maui.ViewModels
{
    public partial class MyIncidentDetailViewModel : ObservableObject
    {
        [ObservableProperty] private IncidentDto incident = default!;

        public IEnumerable<LocationDto> PinItems
            => new[]
            {
                new LocationDto
                {
                    Latitude  = Incident.Latitude,
                    Longitude = Incident.Longitude
                }
            };

        public bool HasHandledAt => Incident.HandledAt.HasValue;

        public MyIncidentDetailViewModel(IIncidentStore store)
        {
            Incident = store.SelectedIncident
                       ?? throw new InvalidOperationException("No incident selected");
            OnPropertyChanged(nameof(HasHandledAt));
            OnPropertyChanged(nameof(PinItems));
        }
    }
}
