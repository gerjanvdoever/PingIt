using System.Net.Http.Json;
using PingIt.Shared.Dtos;

namespace PingIt.Web.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllWorkersAsync();
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
    }

}
