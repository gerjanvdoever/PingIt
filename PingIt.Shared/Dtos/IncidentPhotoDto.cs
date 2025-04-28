namespace PingIt.Shared.Dtos
{
    public class IncidentPhotoDto
    {
        public int Id { get; set; }
        public int IncidentId { get; set; }
        public string PhotoUrl { get; set; } = string.Empty;
    }
}
