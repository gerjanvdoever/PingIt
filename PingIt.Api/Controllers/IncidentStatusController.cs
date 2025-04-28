using Microsoft.AspNetCore.Mvc;
using PingIt.Api.Data;
using PingIt.Shared.Dtos;

namespace PingIt.Api.Controllers
{
    [ApiController]
    [Route("api/incidents/{incidentId}/status")]
    public class IncidentStatusController : ControllerBase
    {
        private readonly PingItDbContext _context;

        public IncidentStatusController(PingItDbContext context)
        {
            _context = context;
        }

        // POST: api/incidents/{incidentId}/status
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(int incidentId, [FromBody] IncidentStatusHistoryDto statusDto)
        {
            return Ok();
        }

        // GET: api/incidents/{incidentId}/status-history
        [HttpGet("history")]
        public async Task<ActionResult<List<IncidentStatusHistoryDto>>> GetStatusHistory(int incidentId)
        {
            return Ok(new List<IncidentStatusHistoryDto>());
        }
    }
}
