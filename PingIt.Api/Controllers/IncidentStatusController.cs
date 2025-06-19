using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PingIt.Api.Data;
using PingIt.Api.Models;
using PingIt.Api.Services;
using PingIt.Api.Services.PingIt.Api.Services;
using PingIt.Shared.Dtos;
using PingIt.Shared.Enums;

namespace PingIt.Api.Controllers
{
    [ApiController]
    [Route("api/incidents/{incidentId}/status")]
    [Authorize(Roles = "Worker, Administrator")]
    public class IncidentStatusController : ControllerBase
    {
        private readonly PingItDbContext _context;
        private readonly IEmailService _emailService;

        public IncidentStatusController(PingItDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // POST: api/incidents/{incidentId}/status
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int incidentId, [FromBody] IncidentStatusUpdateDto statusDto)
        {
            var incident = await _context.Incidents.FindAsync(incidentId);
            if (incident == null)
            {
                return NotFound(new { Message = "Incident not found." });
            }

            bool statusChanged = false;

            if (statusDto.NewStatus.HasValue && incident.Status != statusDto.NewStatus.Value)
            {
                incident.Status = statusDto.NewStatus.Value;
                statusChanged = true;

                // If new status is Resolved -> set HandledAt timestamp
                if (incident.Status == IncidentStatus.Resolved)
                {
                    incident.HandledAt = DateTime.UtcNow;
                }
            }

            if (statusDto.NewWorkerId.HasValue)
            {
                incident.HandledByUserId = statusDto.NewWorkerId;
            }

            if (statusDto.HandledByExternal.HasValue)
            {
                incident.HandledByExternal = statusDto.HandledByExternal.Value;
            }

            if (!string.IsNullOrWhiteSpace(statusDto.Notes))
            {
                incident.Notes = statusDto.Notes;
            }

            if (statusDto.NewPriority.HasValue && incident.Priority != statusDto.NewPriority.Value)
            {
                incident.Priority = statusDto.NewPriority.Value;
            }

            if (statusDto.NewDeadline.HasValue)
            {
                incident.Deadline = statusDto.NewDeadline;
            }


            _context.Incidents.Update(incident);
            await _context.SaveChangesAsync();

            // If status changed, log the change in history
            if (statusChanged)
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userIdClaim == null)
                {
                    return Unauthorized(new { Message = "Invalid token." });
                }

                var userIdFromToken = int.Parse(userIdClaim);

                var statusHistory = new IncidentStatusHistory
                {
                    IncidentId = incident.Id,
                    Status = incident.Status.ToString(),
                    ChangedByUserId = userIdFromToken,
                    ChangedAt = DateTime.UtcNow
                };

                _context.IncidentStatusHistories.Add(statusHistory);
                await _context.SaveChangesAsync();
            }

            // Send notification email to reporter if they want notifications
            if (incident.CreatedByUserId.HasValue)
            {
                var creator = await _context.Users.FindAsync(incident.CreatedByUserId.Value);
                if (creator != null && creator.WantsNotifications)
                {
                    var subject = $"Update on your incident: {incident.Title}";
                    var body = $"Hello {creator.FirstName},\n\n" +
                               $"The status of your reported incident \"{incident.Title}\" has been updated to: {incident.Status}.\n\n" +
                               $"Thank you for helping keep the city safe!\n\n" +
                               $"- PingIt Team";

                    await _emailService.SendEmailAsync(creator.Email, subject, body);
                }
            }

            return Ok(new
            {
                Message = "Incident updated successfully.",
                NewDeadline = incident.Deadline
            });
        }


        // GET: api/incidents/{incidentId}/history
        [HttpGet("history")]
        public async Task<ActionResult<List<IncidentStatusHistoryDto>>> GetStatusHistory(int incidentId)
        {
            var incident = await _context.Incidents.FindAsync(incidentId);
            if (incident == null)
            {
                return NotFound(new { Message = "Incident not found." });
            }

            var history = await _context.IncidentStatusHistories
                .Where(h => h.IncidentId == incidentId)
                .OrderBy(h => h.ChangedAt)
                .ToListAsync();

            var historyDtos = history.Select(h => new IncidentStatusHistoryDto
            {
                Id = h.Id,
                IncidentId = h.IncidentId,
                Status = Enum.Parse<IncidentStatus>(h.Status),
                ChangedByUserId = h.ChangedByUserId,
                ChangedAt = h.ChangedAt
            }).ToList();

            return Ok(historyDtos);
        }
    }
}
