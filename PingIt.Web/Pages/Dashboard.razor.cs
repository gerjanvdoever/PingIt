using Microsoft.AspNetCore.Components;
using PingIt.Web.Pages.Base;
using PingIt.Web.Services;
using PingIt.Shared.Dtos;
using PingIt.Shared.Enums;
using Microsoft.JSInterop;

namespace PingIt.Web.Pages
{
    public partial class Dashboard
    {
        [Inject] private IIncidentService IncidentService { get; set; }
        [Inject] private IAuthService AuthService { get; set; }
        [Inject] private IUserService UserService { get; set; }
        [Inject] private IJSRuntime JS { get; set; }

        private List<UserDto> Workers { get; set; } = new();
        private List<IncidentDto> Incidents { get; set; } = new();
        private List<IncidentDto> ClosedIncidents { get; set; } = new();
        private string CurrentSortColumn = "Status";
        private bool SortAscending = true;
        private bool IsLoading = true;

        private string ClosedSortColumn = "CreatedAt";
        private bool ClosedSortAscending = true;

        private string TitleFilter { get; set; } = "";
        private string ClosedTitleFilter { get; set; } = "";


        private int ActivePage = 1;
        private int ClosedPage = 1;
        private const int PageSize = 50;

        private bool ShowClosedIncidents = false;
        private bool IsClosedLoading = false;

        private IEnumerable<IncidentDto> PagedActive =>
            FilteredActive
                .Skip((ActivePage - 1) * PageSize)
                .Take(PageSize);

        private IEnumerable<IncidentDto> PagedClosed =>
            FilteredClosed
                .Skip((ClosedPage - 1) * PageSize)
                .Take(PageSize);

        private IEnumerable<IncidentDto> FilteredActive =>
        Incidents
        .Where(i => string.IsNullOrWhiteSpace(TitleFilter) || i.Title.Contains(TitleFilter, StringComparison.OrdinalIgnoreCase))
        .ToList();

        private IEnumerable<IncidentDto> FilteredClosed =>
            ClosedIncidents
                .Where(i => string.IsNullOrWhiteSpace(ClosedTitleFilter) || i.Title.Contains(ClosedTitleFilter, StringComparison.OrdinalIgnoreCase))
                .ToList();

        protected override async Task OnInitializedAsync()
        {
            try
            {
                var activeTask = IncidentService.GetActiveIncidentsAsync();
                var closedTask = IncidentService.GetClosedIncidentsAsync();
                var workersTask = UserService.GetAllWorkersAsync();

                await Task.WhenAll(activeTask, closedTask, workersTask);

                Incidents = activeTask.Result;
                Workers = workersTask.Result;

                SortIncidents();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading dashboard data: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                StateHasChanged();
            }
        }

        private async void OnWorkerSelected(IncidentDto incident, string selectedWorkerId)
        {
            if (int.TryParse(selectedWorkerId, out var workerId))
            {
                incident.HandledByUserId = workerId;
            }
            else
            {
                incident.HandledByUserId = null;
            }

            await SaveIncidentAsync(incident);
        }

        private async void OnPriorityChanged(IncidentDto incident, string? newValue)
        {
            if (Enum.TryParse<PriorityLevel>(newValue, out var selectedPriority))
            {
                if (incident.Status == IncidentStatus.Reported && incident.Priority != selectedPriority)
                {
                    incident.Status = IncidentStatus.Registered;
                }

                incident.Priority = selectedPriority;

                incident.Deadline = selectedPriority switch
                {
                    PriorityLevel.Low => DateTime.UtcNow.AddDays(42),
                    PriorityLevel.Normal => DateTime.UtcNow.AddDays(21),
                    PriorityLevel.High => DateTime.UtcNow.AddDays(7),
                    PriorityLevel.Emergency => DateTime.UtcNow.AddDays(1),
                    _ => null
                };

                await SaveIncidentAsync(incident);
            }
        }

        private async void OnExternalSelected(IncidentDto incident, string value)
        {
            if (bool.TryParse(value, out var result))
            {
                incident.HandledByExternal = result;
                await SaveIncidentAsync(incident);
            }
        }

        private void SortBy(string column)
        {
            if (CurrentSortColumn == column)
            {
                SortAscending = !SortAscending;
            }
            else
            {
                CurrentSortColumn = column;
                SortAscending = true;
            }

            SortIncidents();
        }

        private void SortByClosed(string column)
        {
            if (ClosedSortColumn == column)
            {
                ClosedSortAscending = !ClosedSortAscending;
            }
            else
            {
                ClosedSortColumn = column;
                ClosedSortAscending = true;
            }

            ClosedIncidents = ClosedSortColumn switch
            {
                "Title" => ClosedSortAscending ? ClosedIncidents.OrderBy(i => i.Title).ToList() : ClosedIncidents.OrderByDescending(i => i.Title).ToList(),
                "CreatedAt" => ClosedSortAscending ? ClosedIncidents.OrderBy(i => i.CreatedAt).ToList() : ClosedIncidents.OrderByDescending(i => i.CreatedAt).ToList(),
                "Status" => ClosedSortAscending ? ClosedIncidents.OrderBy(i => i.Status).ToList() : ClosedIncidents.OrderByDescending(i => i.Status).ToList(),
                "Priority" => ClosedSortAscending ? ClosedIncidents.OrderBy(i => i.Priority).ToList() : ClosedIncidents.OrderByDescending(i => i.Priority).ToList(),
                "HandledByUserId" => ClosedSortAscending ? ClosedIncidents.OrderBy(i => i.HandledByUserId).ToList() : ClosedIncidents.OrderByDescending(i => i.HandledByUserId).ToList(),
                "HandledByExternal" => ClosedSortAscending ? ClosedIncidents.OrderBy(i => i.HandledByExternal).ToList() : ClosedIncidents.OrderByDescending(i => i.HandledByExternal).ToList(),
                _ => ClosedIncidents
            };
        }

        private async void OnStatusChanged(IncidentDto incident, string? newValue)
        {
            if (Enum.TryParse<IncidentStatus>(newValue, out var selectedStatus))
            {
                incident.Status = selectedStatus;
                await SaveIncidentAsync(incident);
            }
        }

        private async void OnDeadlineChanged(ChangeEventArgs e, IncidentDto incident)
        {
            if (DateTime.TryParse(e.Value?.ToString(), out var newDate))
            {
                incident.Deadline = DateTime.SpecifyKind(newDate, DateTimeKind.Utc);
                await SaveIncidentAsync(incident);
            }
        }

        private string GetWorkerName(int? workerId)
        {
            var worker = Workers.FirstOrDefault(w => w.Id == workerId);
            return worker is null ? "N/A" : $"{worker.FirstName} {worker.LastName}";
        }

        private void SortIncidents()
        {
            Incidents = CurrentSortColumn switch
            {
                "Title" => SortAscending ? Incidents.OrderBy(i => i.Title).ToList() : Incidents.OrderByDescending(i => i.Title).ToList(),
                "CreatedAt" => SortAscending ? Incidents.OrderBy(i => i.CreatedAt).ToList() : Incidents.OrderByDescending(i => i.CreatedAt).ToList(),
                "Status" => SortAscending ? Incidents.OrderBy(i => i.Status).ToList() : Incidents.OrderByDescending(i => i.Status).ToList(),
                "Priority" => SortAscending ? Incidents.OrderBy(i => i.Priority).ToList() : Incidents.OrderByDescending(i => i.Priority).ToList(),
                "HandledByUserId" => SortAscending ? Incidents.OrderBy(i => i.HandledByUserId).ToList() : Incidents.OrderByDescending(i => i.HandledByUserId).ToList(),
                "HandledByExternal" => SortAscending ? Incidents.OrderBy(i => i.HandledByExternal).ToList() : Incidents.OrderByDescending(i => i.HandledByExternal).ToList(),
                _ => Incidents
            };
        }

        private async Task DeleteIncidentAsync(IncidentDto incident)
        {
            var confirmed = await JS.InvokeAsync<bool>(
                "confirm",
                $"Are you sure you want to delete incident \"{incident.Title}\"?"
            );

            if (!confirmed)
                return;

            try
            {
                var success = await IncidentService.DeleteIncidentAsync(incident.Id);

                if (success)
                {
                    Incidents.Remove(incident);
                    SortIncidents();
                    StateHasChanged(); // Trigger re-render to update the map
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Failed to delete incident: {ex.Message}");
            }
        }

        private async Task SaveIncidentAsync(IncidentDto incident)
        {
            var updateDto = new IncidentStatusUpdateDto
            {
                NewStatus = incident.Status,
                NewPriority = incident.Priority,
                NewWorkerId = incident.HandledByUserId,
                HandledByExternal = incident.HandledByExternal,
                NewDeadline = incident.Deadline
            };

            var success = await IncidentService.UpdateIncidentAsync(incident.Id, updateDto);
            if (!success)
            {
                await JS.InvokeVoidAsync("alert", "error changing incident info");
            }
        }

        private async Task LoadClosedIncidentsAsync()
        {
            if (ShowClosedIncidents || IsClosedLoading) return;

            IsClosedLoading = true;
            try
            {
                ClosedIncidents = await IncidentService.GetClosedIncidentsAsync();
                ShowClosedIncidents = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading closed incidents: {ex.Message}");
            }
            finally
            {
                IsClosedLoading = false;
                StateHasChanged();
            }
        }


        private async Task Logout()
        {
            await AuthService.LogoutAsync();
            Nav.NavigateTo("/login", replace: true);
        }

        private void NavigateAdminPanel()
        {
            Nav.NavigateTo("/adminpanel");
        }

        private void NavigateToDetail(int incidentId)
        {
            Nav.NavigateTo($"/incidentdetail/{incidentId}");
        }
    }
}   