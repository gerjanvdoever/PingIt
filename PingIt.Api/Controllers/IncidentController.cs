using Microsoft.AspNetCore.Mvc;
using PingIt.Api.Data;
using PingIt.Shared.Dtos;

namespace PingIt.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IncidentController : ControllerBase
    {
        private readonly PingItDbContext _context;

        public IncidentController(PingItDbContext context)
        {
            _context = context;
        }

        // GET: api/incidents
        [HttpGet]
        public async Task<ActionResult<List<IncidentDto>>> GetAllIncidents()
        {
            // Fetch all incidents (later map to DTOs)
            return Ok(new List<IncidentDto>());
        }

        // GET: api/incidents/active
        [HttpGet("active")]
        public async Task<ActionResult<List<IncidentDto>>> GetActiveIncidents()
        {
            // Fetch active incidents (later map to DTOs)
            return Ok(new List<IncidentDto>());
        }

        // GET: api/incidents/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<IncidentDto>> GetIncident(int id)
        {
            // Find specific incident
            return Ok(new IncidentDto());
        }

        // POST: api/incidents
        [HttpPost]
        public async Task<ActionResult<IncidentDto>> CreateIncident([FromBody] IncidentDto incidentDto)
        {
            // Create new incident
            return CreatedAtAction(nameof(GetIncident), new { id = 1 }, incidentDto);
        }

        // PUT: api/incidents/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIncident(int id, [FromBody] IncidentDto incidentDto)
        {
            // Update incident
            return NoContent();
        }

        // DELETE: api/incidents/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIncident(int id)
        {
            // Delete incident
            return NoContent();
        }
    }
}

