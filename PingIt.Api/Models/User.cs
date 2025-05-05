using PingIt.Shared.Enums;

namespace PingIt.Api.Models
{
    public class User
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public UserRole Role { get; set; } = UserRole.Resident;
        public string? PhoneNumber { get; set; }
        public bool WantsNotifications { get; set; } = false;
        public string Street { get; set; } = null!;
        public string HouseNumber { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public string City { get; set; } = null!;
    }
}
