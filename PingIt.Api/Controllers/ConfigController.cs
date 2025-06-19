using Microsoft.AspNetCore.Mvc;

namespace PingIt.Api.Controllers
{
    [ApiController]
    [Route("api/config")]
    public class ConfigController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ConfigController(IConfiguration config)
        {
            _config = config;
        }

        // retrieving Google Maps API key for .web
        [HttpGet("google-maps-key")]
        public ActionResult<string> GetGoogleMapsApiKey()
        {
            return Ok(new { ApiKey = _config["GoogleMapsApiKey"] });
        }
    }
}
