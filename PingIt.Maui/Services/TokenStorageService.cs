using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace PingIt.Maui.Services
{
    public interface ITokenStorageService
    {
        string? Token { get; }
        int? UserId { get; }
        string? Role { get; }
        bool IsAuthenticated { get; }

        Task StoreTokenAsync(string token, string role);
        Task LoadTokenAsync();
        Task ClearTokenAsync();
    }
    public class TokenStorageService : ITokenStorageService
    {
        private const string TokenKey = "auth_token";
        private const string RoleKey = "auth_role";
        private const string UserIdKey = "auth_userid";

        private string? _token;

        public string? Token => _token;
        public int? UserId { get; private set; }
        public string? Role { get; private set; }
        public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

        public async Task StoreTokenAsync(string token, string role)
        {
            _token = token;
            Role = role;

            await SecureStorage.SetAsync(TokenKey, token);
            await SecureStorage.SetAsync(RoleKey, role);

            ParseJwtClaims(token);

            // Store user ID separately for quick access
            if (UserId.HasValue)
            {
                await SecureStorage.SetAsync(UserIdKey, UserId.Value.ToString());
            }
        }

        public async Task LoadTokenAsync()
        {
            try
            {
                _token = await SecureStorage.GetAsync(TokenKey);
                Role = await SecureStorage.GetAsync(RoleKey);

                var userIdStr = await SecureStorage.GetAsync(UserIdKey);
                if (int.TryParse(userIdStr, out var userId))
                {
                    UserId = userId;
                }

                if (!string.IsNullOrEmpty(_token))
                {
                    ParseJwtClaims(_token);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SecureStorage read error: {ex.Message}");
            }
        }


        public async Task ClearTokenAsync()
        {
            _token = null;
            UserId = null;
            Role = null;
            SecureStorage.Remove(TokenKey);
            await Task.CompletedTask;
        }

        private void ParseJwtClaims(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var userIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub");
            var roleClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role");

            if (int.TryParse(userIdClaim?.Value, out int parsedUserId))
            {
                UserId = parsedUserId;
            }

            Role = roleClaim?.Value;
        }
    }
}
