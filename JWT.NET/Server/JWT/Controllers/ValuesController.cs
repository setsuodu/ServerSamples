using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public IActionResult GetSecretData()
        {
            var username = User.Identity?.Name ?? "Unknown";
            Console.WriteLine($"[C] GetSecretData check Token: {username}");
            return Ok(new { message = $"Hello {username}, this is protected data!" });
        }
    }
}
