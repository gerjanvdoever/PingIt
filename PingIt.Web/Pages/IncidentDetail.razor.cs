using Microsoft.AspNetCore.Components;
using PingIt.Shared.Dtos;
using PingIt.Web.Services;

namespace PingIt.Web.Pages
{
    public partial class IncidentDetail
    {
        [Parameter]
        public int IncidentId { get; set; }

        [Inject]
        private IIncidentService IncidentService { get; set; }

        private IncidentDto? incident;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                incident = await IncidentService.GetIncidentByIdAsync(IncidentId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading incident: {ex.Message}");
            }
        }
    }
}

