using PingIt.Shared.Enums;

namespace PingIt.Shared.Dtos
{
    public class IncidentStatusHistoryDto
    {
        public int Id { get; set; }
        public int IncidentId { get; set; }
        public IncidentStatus Status { get; set; }
        public int ChangedByUserId { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}