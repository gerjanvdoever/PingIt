using Microsoft.AspNetCore.Mvc;
using PingIt.Api.Data;
using PingIt.Shared.Dtos;

namespace PingIt.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly PingItDbContext _context;

        public UserController(PingItDbContext context)
        {
            _context = context;
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            return Ok(new UserDto());
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto userDto)
        {
            return NoContent();
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers()
        {
            return Ok(new List<UserDto>());
        }
    }
}
