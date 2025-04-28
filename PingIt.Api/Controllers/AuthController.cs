using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PingIt.Api.Data;
using PingIt.Api.Services;
using PingIt.Shared.Dtos;

namespace PingIt.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly PingItDbContext _context;
        private readonly JwtService _jwtService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(PingItDbContext context, JwtService jwtService, ILogger<AuthController> logger)
        {
            _context = context;
            _jwtService = jwtService;
            _logger = logger;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null)
            {
                return Unauthorized(new { Message = "User with this email does not exist." });
            }

            var hashedPassword = HashPassword(loginDto.Password);

            if (user.PasswordHash != hashedPassword)
            {
                return Unauthorized(new { Message = "Incorrect password." });
            }

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                Token = token,
                Role = user.Role
            });
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            // TODO: Implement registration logic
            return Ok();
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            var base64Hash = Convert.ToBase64String(hash);

            _logger.LogInformation("[HashPassword] Password: {Password} → Hash: {Hash}", password, base64Hash);


            return base64Hash;
        }
    }
}
