using System.Text;
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
        private List<IncidentStatusHistoryDto> statusHistory = new();
        private Dictionary<int, string> userIdToName = new();
        private UserDto? createdByUser;

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

                statusHistory = await IncidentService.GetStatusHistoryAsync(IncidentId);
                foreach (var entry in statusHistory)
                {
                    await GetUserFullNameAsync(entry.ChangedByUserId);
                }

                if (incident?.CreatedByUserId != null)
                {
                    createdByUser = await UserService.GetUserByIdAsync(incident.CreatedByUserId.Value);
                }
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

        private async Task<string> GetUserFullNameAsync(int userId)
        {
            if (userIdToName.ContainsKey(userId))
                return userIdToName[userId];

            var user = await UserService.GetUserByIdAsync(userId);
            var fullName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown";

            userIdToName[userId] = fullName;
            return fullName;
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

        private async Task ExportToCsv()
        {
            if (incident == null)
                return;

            string FormatValue(object? value) =>
                value switch
                {
                    null => "",
                    DateTime dt => dt.ToString("yyyy-MM-dd HH:mm"),
                    bool b => b.ToString().ToLower(),
                    double d => d.ToString("F5"),
                    _ => $"\"{value.ToString()?.Replace("\"", "\"\"")}\""
                };

            var csv = new StringBuilder();
            csv.AppendLine("Field,Value");

            csv.AppendLine($"Id,{incident.Id}");
            csv.AppendLine($"Title,{FormatValue(incident.Title)}");
            csv.AppendLine($"Description,{FormatValue(incident.Description)}");
            csv.AppendLine($"CreatedAt,{FormatValue(incident.CreatedAt)}");
            csv.AppendLine($"Status,{FormatValue(incident.Status)}");
            csv.AppendLine($"Priority,{FormatValue(incident.Priority)}");
            csv.AppendLine($"Deadline,{FormatValue(incident.Deadline)}");
            csv.AppendLine($"HandledByUserId,{FormatValue(incident.HandledByUserId)}");
            csv.AppendLine($"HandledByExternal,{FormatValue(incident.HandledByExternal)}");
            csv.AppendLine($"Notes,{FormatValue(incident.Notes)}");
            csv.AppendLine($"Latitude,{FormatValue(incident.Latitude)}");
            csv.AppendLine($"Longitude,{FormatValue(incident.Longitude)}");

            var csvBytes = Encoding.UTF8.GetBytes(csv.ToString());
            var base64 = Convert.ToBase64String(csvBytes);

            await JS.InvokeVoidAsync("downloadFile", $"incident-{incident.Id}.csv", base64);
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
