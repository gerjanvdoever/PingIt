using System.Net.Http.Headers;
using System.Net.Http.Json;
using PingIt.Shared.Dtos;

namespace PingIt.Web.Services
{
    public interface IIncidentService
    {
        Task<List<IncidentDto>> GetActiveIncidentsAsync();
        Task<bool> DeleteIncidentAsync(int incidentId);
        Task<bool> UpdateIncidentAsync(int incidentId, IncidentStatusUpdateDto updateDto);
        Task<IncidentDto> GetIncidentByIdAsync(int id);
    }

    public class IncidentService : IIncidentService
    {
        private readonly HttpClient _http;

        public IncidentService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<IncidentDto>> GetActiveIncidentsAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "api/incident/active");
            var response = await _http.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<IncidentDto>>() ?? new List<IncidentDto>();
            }
            else
            {
                throw new HttpRequestException($"Error fetching incidents: {response.ReasonPhrase}");
            }
        }

        public async Task<bool> DeleteIncidentAsync(int incidentId)
        {
            var response = await _http.DeleteAsync($"api/incident/{incidentId}");

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error deleting incident: {error}");
            }
        }

        public async Task<bool> UpdateIncidentAsync(int incidentId, IncidentStatusUpdateDto updateDto)
        {
            var response = await _http.PostAsJsonAsync($"api/incidents/{incidentId}/status", updateDto);

            return response.IsSuccessStatusCode;
        }

        public async Task<IncidentDto> GetIncidentByIdAsync(int id)
        {
            var response = await _http.GetAsync($"api/incident/{id}");

            if (response.IsSuccessStatusCode)
            {
                var incident = await response.Content.ReadFromJsonAsync<IncidentDto>();
                if (incident != null)
                    return incident;

                throw new Exception("Empty response from incident fetch.");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error fetching incident: {error}");
            }
        }
    }

}
