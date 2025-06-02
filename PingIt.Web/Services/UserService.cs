using System.Net.Http.Json;
using PingIt.Shared.Dtos;
using PingIt.Shared.Enums;

namespace PingIt.Web.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllWorkersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<bool> UpdateUserRoleAsync(int id, UserRole role);
    }

    public class UserService : IUserService
    {
        private readonly HttpClient _http;

        public UserService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<UserDto>> GetAllWorkersAsync()
        {
            return await _http.GetFromJsonAsync<List<UserDto>>("api/user/workers")
                   ?? new List<UserDto>();
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var response = await _http.GetAsync($"api/user/{id}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserDto>();
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var response = await _http.GetAsync("api/user");

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException("Failed to retrieve users.");

            return await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? new();
        }

        public async Task<bool> UpdateUserRoleAsync(int id, UserRole role)
        {
            var response = await _http.PutAsJsonAsync($"api/user/role/{id}", new UserRoleDto { Role = role });

            return response.IsSuccessStatusCode;
        }
    }

}
