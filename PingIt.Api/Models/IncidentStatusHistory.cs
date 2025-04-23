namespace PingIt.Api.Models
{
    public class IncidentStatusHistory
    {
        public int Id { get; set; }

        public int IncidentId { get; set; }
        public string Status { get; set; } = null!;
        public int ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}