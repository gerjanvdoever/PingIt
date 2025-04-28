using System.Text.RegularExpressions;
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
    public class UserController : ControllerBase
    {
        private readonly PingItDbContext _context;

        public UserController(PingItDbContext context)
        {
            _context = context;
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            try
            {
                var userIdFromToken = User.GetUserId();

                if (!(userIdFromToken == id || User.IsWorker() || User.IsAdmin()))
                {
                    return Forbid("You are not authorized to view this user's data.");
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

            var userDto = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                WantsNotifications = user.WantsNotifications,
                Street = user.Street,
                HouseNumber = user.HouseNumber,
                PostalCode = user.PostalCode,
                City = user.City,
                Role = Enum.Parse<UserRole>(user.Role)
            };

            return Ok(userDto);
        }

        // PUT: api/user/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto userDto)
        {
            if (id != userDto.Id)
            {
                return BadRequest(new { Message = "User ID mismatch." });
            }

            try
            {
                var userIdFromToken = User.GetUserId();

                if (!User.IsAdmin() && userIdFromToken != id)
                {
                    return Forbid("You are not authorized to update this user.");
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

            var validationResult = ValidateUserDto(userDto);
            if (!string.IsNullOrEmpty(validationResult))
            {
                return BadRequest(new { Message = validationResult });
            }

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.PhoneNumber = userDto.PhoneNumber;
            user.WantsNotifications = userDto.WantsNotifications;
            user.Street = userDto.Street;
            user.HouseNumber = userDto.HouseNumber;
            user.PostalCode = userDto.PostalCode;
            user.City = userDto.City;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/user
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();

            var userDtos = users.Select(user => new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                WantsNotifications = user.WantsNotifications,
                Street = user.Street,
                HouseNumber = user.HouseNumber,
                PostalCode = user.PostalCode,
                City = user.City,
                Role = Enum.Parse<UserRole>(user.Role)
            }).ToList();

            return Ok(userDtos);
        }

        // GET: api/user/workers
        [HttpGet("workers")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<List<UserDto>>> GetAllWorkers()
        {
            var workers = await _context.Users
                .Where(u => u.Role == UserRole.Worker.ToString())
                .Select(user => new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = Enum.Parse<UserRole>(user.Role)
                })
                .ToListAsync();

            return Ok(workers);
        }


        // PUT: api/user/role/{id}
        [HttpPut("role/{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UserRoleDto userRoleDto)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
            }

            user.Role = userRoleDto.Role.ToString();

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string ValidateUserDto(UserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FirstName) ||
                string.IsNullOrWhiteSpace(dto.LastName) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
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

            var postalCodeRegex = new Regex(@"^\d{4}[A-Z]{2}$");
            if (!postalCodeRegex.IsMatch(dto.PostalCode))
            {
                return "Postal code must be in the format 1234AB.";
            }

            return string.Empty;
        }
    }
}