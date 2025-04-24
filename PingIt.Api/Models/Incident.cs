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

        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = IncidentStatus.Reported.ToString();
        public string Priority { get; set; } = PriorityLevel.Unknown.ToString();
        public DateTime? Deadline { get; set; }
        public DateTime? HandledAt { get; set; }
        public List <IncidentPhoto> Photos { get; set; } = new();

        // Can be null if the incident is reported anonymously
        public int? CreatedByUserId { get; set; }
    }
}