using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PingIt.Api.Data;
using PingIt.Shared.Dtos;

// Note:
// [Authorize] <-- can be used both at controller and action level
// [Authorize(Roles = "User, Admin")] <-- can be used to specify roles
// Resident, Worker, Administrator
// Authorize at controller level CAN be overridden at action level

namespace PingIt.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IncidentController : ControllerBase
    {
        private readonly PingItDbContext _context;

        public IncidentController(PingItDbContext context)
        {
            _context = context;
        }

        // all Incidents
        // GET: api/incidents
        [HttpGet]
        [Authorize(Roles = "Worker, Administrator")]
        public async Task<ActionResult<List<IncidentDto>>> GetAllIncidents()
        {
            // Fetch all incidents (later map to DTOs)
            return Ok(new List<IncidentDto>());
        }

        // Only active Incidents
        // GET: api/incidents/active
        [HttpGet("active")]
        [Authorize(Roles = "Worker, Administrator")]
        public async Task<ActionResult<List<IncidentDto>>> GetActiveIncidents()
        {
            // Fetch active incidents (later map to DTOs)
            return Ok(new List<IncidentDto>());
        }

        // Incidents by Id
        // GET: api/incidents/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<IncidentDto>> GetIncident(int id)
        {
            // Find specific incident
            return Ok(new IncidentDto());
        }

        // Incidents by UserId
        // GET: api/incidents/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<IncidentDto>>> GetIncidentsByUserId(int userId)
        {
            // Fetch incidents by userId (later map to DTOs)
            return Ok(new List<IncidentDto>());
        }

        // POST: api/incidents
        [HttpPost]
        public async Task<ActionResult<IncidentDto>> CreateIncident([FromBody] IncidentDto incidentDto)
        {
            // Create new incident
            return CreatedAtAction(nameof(GetIncident), new { id = 1 }, incidentDto);
        }

        // DELETE: api/incidents/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteIncident(int id)
        {
            // Delete incident
            return NoContent();
        }
    }
}

