namespace PingIt.Shared.Dtos
{
    public class IncidentStatusUpdateDto
    {
        public string? NewStatus { get; set; } 
        public int? NewWorkerId { get; set; }
        public bool? HandledByExternal { get; set; } 
        public string? Notes { get; set; }
        public string? NewPriority { get; set; }
    }
}
