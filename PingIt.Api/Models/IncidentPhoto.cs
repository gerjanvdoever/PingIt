namespace PingIt.Api.Models
{
    public class IncidentPhoto
    {
        public int Id { get; set; }

        public int IncidentId { get; set; }
        public string PhotoUrl { get; set; } = null!;
    }
}
