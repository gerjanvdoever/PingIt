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
                Role = user.Role
            };

            return Ok(userDto);
        }

        // PUT: api/user/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto userDto)
        {
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

            // Apply updates only if values are provided
            if (userDto.Email != null) user.Email = userDto.Email;
            if (userDto.FirstName != null) user.FirstName = userDto.FirstName;
            if (userDto.LastName != null) user.LastName = userDto.LastName;
            if (userDto.PhoneNumber != null) user.PhoneNumber = userDto.PhoneNumber;
            if (userDto.WantsNotifications.HasValue) user.WantsNotifications = userDto.WantsNotifications.Value;
            if (userDto.Street != null) user.Street = userDto.Street;
            if (userDto.HouseNumber != null) user.HouseNumber = userDto.HouseNumber;
            if (userDto.PostalCode != null) user.PostalCode = userDto.PostalCode;
            if (userDto.City != null) user.City = userDto.City;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok();
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
                Role = user.Role
            }).ToList();

            return Ok(userDtos);
        }

        // GET: api/user/workers
        [HttpGet("workers")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<List<UserDto>>> GetAllWorkers()
        {
            var workers = await _context.Users
                .Where(u => u.Role == UserRole.Worker)
                .Select(user => new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role
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

            user.Role = userRoleDto.Role;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string ValidateUserDto(UpdateUserDto dto)
        {
            // Check string fields only if they are provided (non-null)
            if (dto.FirstName is { Length: 0 } ||
                dto.LastName is { Length: 0 } ||
                dto.Street is { Length: 0 } ||
                dto.HouseNumber is { Length: 0 } ||
                dto.PostalCode is { Length: 0 } ||
                dto.City is { Length: 0 })
            {
                return "Fields cannot be empty if provided.";
            }

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var emailRegex = new Regex(@"^\S+@\S+\.\S+$");
                if (!emailRegex.IsMatch(dto.Email))
                {
                    return "Invalid email format.";
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.PostalCode))
            {
                var postalCodeRegex = new Regex(@"^\d{4}[A-Z]{2}$");
                if (!postalCodeRegex.IsMatch(dto.PostalCode))
                {
                    return "Postal code must be in the format 1234AB.";
                }
            }

            return string.Empty;
        }
    }
}