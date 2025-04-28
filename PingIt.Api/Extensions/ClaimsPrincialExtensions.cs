using System.Security.Claims;
using PingIt.Shared.Enums;

namespace PingIt.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("Invalid token: no user ID found.");
            }

            return int.Parse(userIdClaim);
        }

        public static string GetRole(this ClaimsPrincipal user)
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
            if (roleClaim == null)
            {
                throw new UnauthorizedAccessException("Invalid token: no role found.");
            }

            return roleClaim;
        }

        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.GetRole() == UserRole.Administrator.ToString();
        }

        public static bool IsWorker(this ClaimsPrincipal user)
        {
            return user.GetRole() == UserRole.Worker.ToString();
        }

        public static bool IsResident(this ClaimsPrincipal user)
        {
            return user.GetRole() == UserRole.Resident.ToString();
        }
    }
}
