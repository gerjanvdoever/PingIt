using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PingIt.Api.Data;
using PingIt.Api.Models;
using PingIt.Api.Services;
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
        private readonly EmailService _emailService;

        public IncidentStatusController(PingItDbContext context, EmailService emailService)
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

            // 1. Status change
            if (!string.IsNullOrEmpty(statusDto.NewStatus) && incident.Status != statusDto.NewStatus)
            {
                incident.Status = statusDto.NewStatus;
                statusChanged = true;

                // If new status is Resolved -> set HandledAt timestamp
                if (statusDto.NewStatus == IncidentStatus.Resolved.ToString())
                {
                    incident.HandledAt = DateTime.UtcNow;

                    // TODO: Send notification email to reporter (if WantsNotifications = true & CreatedByUserid != null)
                }
            }

            // 2. Assign to worker
            if (statusDto.NewWorkerId.HasValue)
            {
                incident.HandledByUserId = statusDto.NewWorkerId;
            }

            // 3. Mark as handled externally
            if (statusDto.HandledByExternal.HasValue)
            {
                incident.HandledByExternal = statusDto.HandledByExternal.Value;
            }

            // 4. Update notes
            if (!string.IsNullOrEmpty(statusDto.Notes))
            {
                incident.Notes = statusDto.Notes;
            }

            // 5. Priority change
            if (!string.IsNullOrEmpty(statusDto.NewPriority) && incident.Priority != statusDto.NewPriority)
            {
                incident.Priority = statusDto.NewPriority;

                // Only if we move away from Unknown, calculate deadline
                if (Enum.TryParse<PriorityLevel>(statusDto.NewPriority, out var priorityLevel) &&
                    priorityLevel != PriorityLevel.Unknown)
                {
                    incident.Deadline = priorityLevel switch
                    {
                        PriorityLevel.Low => DateTime.UtcNow.AddDays(42),      // 6 weeks
                        PriorityLevel.Normal => DateTime.UtcNow.AddDays(21),   // 3 weeks
                        PriorityLevel.High => DateTime.UtcNow.AddDays(7),      // 1 week
                        PriorityLevel.Emergency => DateTime.UtcNow.AddDays(1), // 1 day
                        _ => null
                    };
                }
            }

            // Save changes to incident
            _context.Incidents.Update(incident);
            await _context.SaveChangesAsync();

            // Create IncidentStatusHistory if status was changed
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
                    Status = incident.Status,
                    ChangedByUserId = userIdFromToken,
                    ChangedAt = DateTime.UtcNow
                };

                _context.IncidentStatusHistories.Add(statusHistory);
                await _context.SaveChangesAsync();
            }

            // Send notification email to reporter (if WantsNotifications = true & CreatedByUserid != null)
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

            return Ok(new { Message = "Incident updated successfully." });
        }

        // GET: api/incidents/{incidentId}/status-history
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
