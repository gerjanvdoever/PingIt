using PingIt.Shared.Enums;

namespace PingIt.Shared.Dtos
{
    public class IncidentDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public DateTime CreatedAt { get; set; }
        public IncidentStatus Status { get; set; } = IncidentStatus.Reported;
        public PriorityLevel Priority { get; set; } = PriorityLevel.Unknown;
        public DateTime? Deadline { get; set; }
        public DateTime? HandledAt { get; set; }

        public int? CreatedByUserId { get; set; }
        public bool HandledByExternal { get; set; } = false;
        public int? HandledByUserId { get; set; }
        public string? Notes { get; set; }
    }
}
