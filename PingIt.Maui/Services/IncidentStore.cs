using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PingIt.Shared.Dtos;

namespace PingIt.Maui.Services
{
    public interface IIncidentStore
    {
        IncidentDto? SelectedIncident { get; set; }
    }

    // Used for persisting incident data across screens
    public class IncidentStore : IIncidentStore
    {
        public IncidentDto? SelectedIncident { get; set; }
    }
}
