using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PingIt.Api.Data;
using PingIt.Api.Extensions;
using PingIt.Api.Models;
using PingIt.Shared.Dtos;
using PingIt.Shared.Enums;

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

        // GET: api/incidents
        [HttpGet]
        [Authorize(Roles = "Worker, Administrator")]
        public async Task<ActionResult<List<IncidentDto>>> GetAllIncidents()
        {
            var incidents = await _context.Incidents.ToListAsync();

            var incidentDtos = incidents.Select(incident => MapToDto(incident)).ToList();

            return Ok(incidentDtos);
        }

        // GET: api/incidents/active
        [HttpGet("active")]
        [Authorize(Roles = "Worker, Administrator")]
        public async Task<ActionResult<List<IncidentDto>>> GetActiveIncidents()
        {
            var incidents = await _context.Incidents
                .Where(i => i.Status != IncidentStatus.Resolved.ToString())
                .ToListAsync();

            var incidentDtos = incidents.Select(incident => MapToDto(incident)).ToList();

            return Ok(incidentDtos);
        }

        // GET: api/incidents/closed
        [HttpGet("closed")]
        [Authorize(Roles = "Worker, Administrator")]
        public async Task<ActionResult<List<IncidentDto>>> GetClosedIncidents()
        {
            var incidents = await _context.Incidents
                .Where(i => i.Status == IncidentStatus.Resolved.ToString())
                .ToListAsync();

            var incidentDtos = incidents.Select(incident => MapToDto(incident)).ToList();

            return Ok(incidentDtos);
        }

        // GET: api/incidents/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<IncidentDto>> GetIncident(int id)
        {
            var incident = await _context.Incidents.FindAsync(id);

            if (incident == null)
            {
                return NotFound(new { Message = "Incident not found." });
            }

            return Ok(MapToDto(incident));
        }

        // GET: api/incidents/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<IncidentDto>>> GetIncidentsByUserId(int userId)
        {
            var incidents = await _context.Incidents
                .Where(i => i.CreatedByUserId == userId)
                .ToListAsync();

            var incidentDtos = incidents.Select(incident => MapToDto(incident)).ToList();

            return Ok(incidentDtos);
        }

        // GET: api/incidents/worker/{workerId}
        [HttpGet("worker/{workerId}")]
        [Authorize(Roles = "Worker, Administrator")]
        public async Task<ActionResult<List<IncidentDto>>> GetIncidentsByWorkerId(int workerId)
        {
            var incidents = await _context.Incidents
                .Where(i => i.HandledByUserId == workerId)
                .ToListAsync();

            var incidentDtos = incidents.Select(incident => MapToDto(incident)).ToList();

            return Ok(incidentDtos);
        }

        // POST: api/incidents
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<IncidentDto>> CreateIncident([FromBody] IncidentDto incidentDto)
        {
            var incident = new Incident
            {
                Title = incidentDto.Title,
                Description = incidentDto.Description,
                Latitude = incidentDto.Latitude,
                Longitude = incidentDto.Longitude,
                CreatedAt = DateTime.UtcNow,
                Status = IncidentStatus.Reported.ToString(),
                Priority = PriorityLevel.Unknown.ToString(),
                CreatedByUserId = incidentDto.CreatedByUserId,
                HandledByExternal = false
            };

            _context.Incidents.Add(incident);
            await _context.SaveChangesAsync();

            var createdDto = MapToDto(incident);

            return CreatedAtAction(nameof(GetIncident), new { id = incident.Id }, createdDto);
        }

        // DELETE: api/incidents/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteIncident(int id)
        {
            var incident = await _context.Incidents.FindAsync(id);
            if (incident == null)
            {
                return NotFound(new { Message = "Incident not found." });
            }

            _context.Incidents.Remove(incident);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private IncidentDto MapToDto(Incident incident)
        {
            return new IncidentDto
            {
                Id = incident.Id,
                Title = incident.Title,
                Description = incident.Description,
                Latitude = incident.Latitude,
                Longitude = incident.Longitude,
                CreatedAt = incident.CreatedAt,
                Status = Enum.Parse<IncidentStatus>(incident.Status),
                Priority = Enum.Parse<PriorityLevel>(incident.Priority),
                Deadline = incident.Deadline,
                HandledAt = incident.HandledAt,
                CreatedByUserId = incident.CreatedByUserId,
                HandledByExternal = incident.HandledByExternal,
                HandledByUserId = incident.HandledByUserId,
                Notes = incident.Notes
            };
        }
    }
}
