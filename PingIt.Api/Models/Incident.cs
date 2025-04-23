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
        public string Status { get; set; } = null!;
        public string Priority { get; set; } = null!;
        public DateTime Deadline { get; set; }
        public DateTime? HandledAt { get; set; }

        public int? CreatedByUserId { get; set; }
    }
}