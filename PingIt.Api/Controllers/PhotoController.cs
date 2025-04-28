using Microsoft.AspNetCore.Mvc;
using PingIt.Api.Data;
using PingIt.Shared.Dtos;

namespace PingIt.Api.Controllers
{
    [ApiController]
    [Route("api/incidents/{incidentId}/photos")]
    public class PhotoController : ControllerBase
    {
        private readonly PingItDbContext _context;

        public PhotoController(PingItDbContext context)
        {
            _context = context;
        }

        // POST: api/incidents/{incidentId}/photos
        [HttpPost]
        public async Task<IActionResult> UploadPhoto(int incidentId, [FromBody] IncidentPhotoDto photoDto)
        {
            return Ok();
        }

        // DELETE: api/photos/{photoId}
        [HttpDelete("~/api/photos/{photoId}")]
        public async Task<IActionResult> DeletePhoto(int photoId)
        {
            return NoContent();
        }
    }
}
