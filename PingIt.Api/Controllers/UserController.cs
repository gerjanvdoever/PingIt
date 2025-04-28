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
                City = user.City
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

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { Message = "User not found." });
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
    }
}
