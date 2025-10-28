using Microsoft.AspNetCore.Mvc;
using EFCore.DTOs.Auth;
using EFCore.Services;

namespace EFCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }

        // 可扩展：注册、刷新 token、退出登录等
    }
}
