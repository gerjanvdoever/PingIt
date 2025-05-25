using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PingIt.Api.Data;
using PingIt.Api.Services;
using PingIt.Api.Extensions;
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
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return Conflict(new { Message = "A user with this email already exists." });
            }

            var validationResult = ValidateRegisterDto(registerDto);
            if (!string.IsNullOrEmpty(validationResult))
            {
                return BadRequest(new { Message = validationResult });
            }

            var user = new Models.User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password),
                Role = Shared.Enums.UserRole.Resident,
                PhoneNumber = registerDto.PhoneNumber,
                WantsNotifications = registerDto.WantsNotifications,
                Street = registerDto.Street,
                HouseNumber = registerDto.HouseNumber,
                PostalCode = registerDto.PostalCode,
                City = registerDto.City
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                Token = token,
                Role = user.Role.ToString(),
                Message = "User registered successfully."
            });
        }

        // PUT: api/auth/change-password/{id}
        [HttpPut("change-password/{id}")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto passwordDto)
        {
            try
            {
                var userIdFromToken = User.GetUserId();

                if (userIdFromToken != id)
                {
                    return Forbid("You are not authorized to change another user's password.");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            // Check old password
            var hashedOldPassword = HashPassword(passwordDto.OldPassword);
            if (user.PasswordHash != hashedOldPassword)
            {
                return Unauthorized(new { Message = "Old password is incorrect." });
            }

            // Check that new password and confirmation match
            if (passwordDto.NewPassword != passwordDto.ConfirmPassword)
            {
                return BadRequest(new { Message = "New password and confirmation do not match." });
            }

            // Validate new password format
            var passwordRegex = new Regex(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9]).{8,}$");
            if (!passwordRegex.IsMatch(passwordDto.NewPassword))
            {
                return BadRequest(new
                {
                    Message = "New password must be at least 8 characters long, contain an uppercase letter, a lowercase letter and a number."
                });
            }

            user.PasswordHash = HashPassword(passwordDto.NewPassword);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Password changed successfully." });
        }


        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private string ValidateRegisterDto(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FirstName) ||
                string.IsNullOrWhiteSpace(dto.LastName) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password) ||
                string.IsNullOrWhiteSpace(dto.Street) ||
                string.IsNullOrWhiteSpace(dto.HouseNumber) ||
                string.IsNullOrWhiteSpace(dto.PostalCode) ||
                string.IsNullOrWhiteSpace(dto.City))
            {
                return "All fields are required.";
            }

            var emailRegex = new Regex(@"^\S+@\S+\.\S+$");
            if (!emailRegex.IsMatch(dto.Email))
            {
                return "Invalid email format.";
            }

            var passwordRegex = new Regex(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?[0-9]).{8,}$");
            if (!passwordRegex.IsMatch(dto.Password))
            {
                return "Password must be at least 8 characters long, contain an uppercase letter, a lowercase letter and a number.";
            }

            var postalCodeRegex = new Regex(@"^\d{4}[A-Z]{2}$");
            if (!postalCodeRegex.IsMatch(dto.PostalCode))
            {
                return "Postal code must be in the format 1234AB.";
            }

            return string.Empty;
        }
    }
}
