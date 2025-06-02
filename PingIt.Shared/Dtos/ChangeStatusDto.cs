using PingIt.Shared.Enums;

namespace PingIt.Shared.Dtos
{
    public class IncidentStatusUpdateDto
    {
        public IncidentStatus? NewStatus { get; set; }
        public int? NewWorkerId { get; set; }
        public DateTime? NewDeadline { get; set; }
        public bool? HandledByExternal { get; set; } 
        public string? Notes { get; set; }
        public PriorityLevel? NewPriority { get; set; }
    }
}
