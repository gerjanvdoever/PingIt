using PingIt.Shared.Enums;

namespace PingIt.Api.Models
{
    public class Incident
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public List<IncidentPhoto> Photos { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public IncidentStatus Status { get; set; } = IncidentStatus.Reported;
        public PriorityLevel Priority { get; set; } = PriorityLevel.Unknown;
        public DateTime? Deadline { get; set; }
        public DateTime? HandledAt { get; set; }
        public int? CreatedByUserId { get; set; } // Can be null if the incident is reported anonymously
        public bool HandledByExternal { get; set; } = false;
        public int? HandledByUserId { get; set; } // Can be null if the incident is not handled yet or if it is handled by an external party
        public string? Notes { get; set; }
    }
}