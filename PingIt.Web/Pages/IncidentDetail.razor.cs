using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PingIt.Shared.Dtos;
using PingIt.Shared.Enums;
using PingIt.Web.Pages.Base;
using PingIt.Web.Services;

namespace PingIt.Web.Pages
{
    public partial class IncidentDetail : BaseAdminPage
    {
        [Parameter]
        public int IncidentId { get; set; }

        [Inject] private IIncidentService IncidentService { get; set; }
        [Inject] private IUserService UserService { get; set; }
        [Inject] private IJSRuntime JS { get; set; }

        private IncidentDto? incident;
        private List<UserDto> Workers = new();
        private string? deadlineString;
        private List<IncidentDto> singleIncidentList = new();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var incidentTask = IncidentService.GetIncidentByIdAsync(IncidentId);
                var workersTask = UserService.GetAllWorkersAsync();

                await Task.WhenAll(incidentTask, workersTask);

                incident = incidentTask.Result;
                Workers = workersTask.Result;
                if (incident != null)
                {
                    singleIncidentList = new List<IncidentDto> { incident };
                }

                deadlineString = incident?.Deadline?.ToString("yyyy-MM-ddTHH:mm");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading incident: {ex.Message}");
            }
        }

        private void GoBack()
        {
            Nav.NavigateTo("/dashboard");
        }

        private void OnPriorityChanged(ChangeEventArgs e)
        {
            if (incident == null) return;

            var newValue = e.Value?.ToString();
            if (Enum.TryParse<PriorityLevel>(newValue, out var selected))
            {
                incident.Priority = selected;

                // Automatically update deadline based on priority
                incident.Deadline = selected switch
                {
                    PriorityLevel.Low => DateTime.UtcNow.AddDays(42),
                    PriorityLevel.Normal => DateTime.UtcNow.AddDays(21),
                    PriorityLevel.High => DateTime.UtcNow.AddDays(7),
                    PriorityLevel.Emergency => DateTime.UtcNow.AddDays(1),
                    _ => null
                };

                deadlineString = incident.Deadline?.ToString("yyyy-MM-ddTHH:mm");
            }
        }

        private void OnDeadlineChanged(ChangeEventArgs e)
        {
            deadlineString = e.Value?.ToString();

            if (incident == null || string.IsNullOrWhiteSpace(deadlineString))
                return;

            if (DateTime.TryParse(deadlineString, out var parsed))
            {
                incident.Deadline = DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
            }
        }

        private async Task ConfirmDeleteAsync()
        {
            bool confirmed = await JS.InvokeAsync<bool>("confirm", "Are you sure you want to delete this incident?");
            if (confirmed)
            {
                try
                {
                    var result = await IncidentService.DeleteIncidentAsync(incident!.Id);
                    if (result)
                    {
                        await JS.InvokeVoidAsync("alert", "Incident deleted successfully.");
                        Nav.NavigateTo("/dashboard");
                    }
                }
                catch (Exception ex)
                {
                    await JS.InvokeVoidAsync("alert", $"Error deleting incident: {ex.Message}");
                }
            }
        }


        private async Task SaveChangesAsync()
        {
            if (incident == null) return;

            var updateDto = new IncidentStatusUpdateDto
            {
                NewStatus = incident.Status,
                NewPriority = incident.Priority,
                NewWorkerId = incident.HandledByUserId,
                HandledByExternal = incident.HandledByExternal,
                NewDeadline = incident.Deadline,
                Notes = incident.Notes
            };

            var success = await IncidentService.UpdateIncidentAsync(incident.Id, updateDto);

            if (!success)
            {
                await JS.InvokeVoidAsync("alert", "Error saving incident.");
            }
        }
    }
}
