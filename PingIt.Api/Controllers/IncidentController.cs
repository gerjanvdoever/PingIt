using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PingIt.Api.Data;
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
            var incidents = await _context.Incidents
                .Include(i => i.Photos)
                .ToListAsync();

            return Ok(incidents.Select(MapToDto).ToList());
        }

        // GET: api/incidents/active
        [HttpGet("active")]
        [Authorize(Roles = "Worker, Administrator")]
        public async Task<ActionResult<List<IncidentDto>>> GetActiveIncidents()
        {
            var incidents = await _context.Incidents
                .Include(i => i.Photos)
                .Where(i => i.Status != IncidentStatus.Resolved)
                .ToListAsync();

            return Ok(incidents.Select(MapToDto).ToList());
        }

        // GET: api/incidents/closed
        [HttpGet("closed")]
        [Authorize(Roles = "Worker, Administrator")]
        public async Task<ActionResult<List<IncidentDto>>> GetClosedIncidents()
        {
            var incidents = await _context.Incidents
                .Include(i => i.Photos)
                .Where(i => i.Status == IncidentStatus.Resolved)
                .ToListAsync();

            return Ok(incidents.Select(MapToDto).ToList());
        }

        // GET: api/incidents/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<IncidentDto>> GetIncident(int id)
        {
            var incident = await _context.Incidents
                .Include(i => i.Photos)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (incident == null)
                return NotFound(new { Message = "Incident not found." });

            return Ok(MapToDto(incident));
        }

        // GET: api/incidents/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<IncidentDto>>> GetIncidentsByUserId(int userId)
        {
            var incidents = await _context.Incidents
                .Include(i => i.Photos)
                .Where(i => i.CreatedByUserId == userId)
                .ToListAsync();

            return Ok(incidents.Select(MapToDto).ToList());
        }

        // GET: api/incidents/worker/{workerId}/closed
        [HttpGet("worker/{workerId}/closed")]
        [Authorize(Roles = "Worker, Administrator")]
        public async Task<ActionResult<List<IncidentDto>>> GetClosedIncidentsByWorkerId(int workerId)
        {
            var incidents = await _context.Incidents
                .Include(i => i.Photos)
                .Where(i => i.HandledByUserId == workerId && i.Status == IncidentStatus.Resolved)
                .ToListAsync();

            return Ok(incidents.Select(MapToDto).ToList());
        }

        // GET: api/incidents/worker/{workerId}/active
        [HttpGet("worker/{workerId}/active")]
        [Authorize(Roles = "Worker, Administrator")]
        public async Task<ActionResult<List<IncidentDto>>> GetActiveIncidentsByWorkerId(int workerId)
        {
            var incidents = await _context.Incidents
                .Include(i => i.Photos)
                .Where(i => i.HandledByUserId == workerId && i.Status != IncidentStatus.Resolved)
                .ToListAsync();

            return Ok(incidents.Select(MapToDto).ToList());
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
                Status = IncidentStatus.Reported,
                Priority = PriorityLevel.Unknown,
                CreatedByUserId = incidentDto.CreatedByUserId,
                HandledByExternal = false
            };

            _context.Incidents.Add(incident);
            await _context.SaveChangesAsync();

            // reload with photos (will be empty at creation)
            await _context.Entry(incident).Collection(i => i.Photos).LoadAsync();

            return CreatedAtAction(
                nameof(GetIncident),
                new { id = incident.Id },
                MapToDto(incident));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteIncident(int id)
        {
            var incident = await _context.Incidents.FindAsync(id);
            if (incident == null)
                return NotFound(new { Message = "Incident not found." });

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isAdmin = User.IsInRole("Administrator");

            if (!isAdmin)
            {
                if (incident.CreatedByUserId != userId)
                    return Forbid();

                if (incident.Status != IncidentStatus.Reported)
                    return BadRequest(new { Message = "Only 'Reported' incidents may be deleted." });
            }

            _context.Incidents.Remove(incident);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/incidents/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<IncidentDto>> UpdateIncident(
            int id,
            [FromBody] IncidentUpdateDto updateDto)
        {
            var incident = await _context.Incidents.FindAsync(id);
            if (incident == null)
                return NotFound(new { Message = "Incident not found." });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId)
                || incident.CreatedByUserId != userId)
            {
                return Forbid();
            }

            incident.Title = updateDto.Title;
            incident.Description = updateDto.Description;

            await _context.SaveChangesAsync();

            // ensure photos are loaded before mapping
            await _context.Entry(incident).Collection(i => i.Photos).LoadAsync();

            return Ok(MapToDto(incident));
        }

        private IncidentDto MapToDto(Incident incident)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            return new IncidentDto
            {
                Id = incident.Id,
                Title = incident.Title,
                Description = incident.Description,
                Photos = incident.Photos
                                     .Select(p => new IncidentPhotoDto
                                     {
                                         Id = p.Id,
                                         IncidentId = p.IncidentId,
                                         PhotoUrl = baseUrl + p.PhotoUrl
                                     })
                                     .ToList(),
                Latitude = incident.Latitude,
                Longitude = incident.Longitude,
                CreatedAt = incident.CreatedAt,
                Status = incident.Status,
                Priority = incident.Priority,
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
