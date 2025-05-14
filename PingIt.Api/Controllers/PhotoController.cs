using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PingIt.Api.Data;
using PingIt.Api.Models;
using PingIt.Shared.Dtos;

namespace PingIt.Api.Controllers
{
    [ApiController]
    [Route("api/incident/{incidentId}/photos")]
    public class PhotoController : ControllerBase
    {
        private readonly PingItDbContext _context;

        public PhotoController(PingItDbContext context)
        {
            _context = context;
        }

        // POST: api/incident/{incidentId}/photos
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> UploadPhoto(int incidentId, [FromBody] IncidentPhotoDto photoDto)
        {
            var incident = await _context.Incidents.FindAsync(incidentId);
            if (incident == null)
            {
                return NotFound(new { Message = "Incident not found." });
            }

            if (string.IsNullOrEmpty(photoDto.PhotoUrl))
            {
                return BadRequest(new { Message = "No photo data provided." });
            }

            // Create uploads folder if it doesn't exist
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Decode base64 to byte array
            byte[] imageBytes;
            try
            {
                imageBytes = Convert.FromBase64String(photoDto.PhotoUrl);
            }
            catch
            {
                return BadRequest(new { Message = "Invalid base64 photo data." });
            }

            // Generate a unique filename
            var fileName = $"incident-{incidentId}-{Guid.NewGuid()}.jpg";
            var filePath = Path.Combine(uploadPath, fileName);

            await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

            // Save to database
            var newPhoto = new IncidentPhoto
            {
                IncidentId = incidentId,
                PhotoUrl = $"/uploads/{fileName}" // Save the relative path
            };

            _context.IncidentPhotos.Add(newPhoto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPhotos), new { incidentId = incidentId }, new { Message = "Photo uploaded successfully.", PhotoUrl = newPhoto.PhotoUrl });
        }

        // GET: api/incident/{incidentId}/photos
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<IncidentPhotoDto>>> GetPhotos(int incidentId)
        {
            var incident = await _context.Incidents.FindAsync(incidentId);

            if (incident == null)
            {
                return NotFound(new { Message = "Incident not found." });
            }

            var photos = await _context.IncidentPhotos
                .Where(p => p.IncidentId == incidentId)
                .ToListAsync();

            var photoDtos = photos.Select(p => new IncidentPhotoDto
            {
                Id = p.Id,
                IncidentId = p.IncidentId,
                PhotoUrl = p.PhotoUrl
            }).ToList();

            return Ok(photoDtos);
        }

        // DELETE: api/incidents/{incidentId}/photos/{photoId}
        [HttpDelete("{photoId}")]
        [Authorize(Roles = "Worker, Administrator")]
        public async Task<IActionResult> DeletePhoto(int incidentId, int photoId)
        {
            var incident = await _context.Incidents.FindAsync(incidentId);
            if (incident == null)
            {
                return NotFound(new { Message = "Incident not found." });
            }

            var photo = await _context.IncidentPhotos
                .FirstOrDefaultAsync(p => p.Id == photoId && p.IncidentId == incidentId);

            if (photo == null)
            {
                return NotFound(new { Message = "Photo not found for this incident." });
            }

            _context.IncidentPhotos.Remove(photo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
