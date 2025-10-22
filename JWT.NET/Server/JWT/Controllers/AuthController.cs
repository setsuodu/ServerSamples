using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        Console.WriteLine($"[C] Login: {request.Username}:{request.Password}");

        // 示例验证逻辑：实际应查数据库
        if (request.Username == "admin" && request.Password == "123456")
        {
            var accessToken = GenerateJwtToken(request.Username);
            var refreshToken = TokenStorage.Create(request.Username, TimeSpan.FromDays(7));

            return Ok(new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }
        return Unauthorized();
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshRequest request)
    {
        var existing = TokenStorage.Validate(request.RefreshToken);
        Console.WriteLine($"Refresh: {existing == null}");
        if (existing == null)
            return Unauthorized("Refresh token invalid or expired");

        // 可选：撤销旧 refresh token（防止重放）
        TokenStorage.Revoke(request.RefreshToken);

        var newAccessToken = GenerateJwtToken(existing.Username);
        var newRefreshToken = TokenStorage.Create(existing.Username, TimeSpan.FromDays(7));

        return Ok(new LoginResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        });
    }

    private string GenerateJwtToken(string username)
    {
        var jwtSettings = _config.GetSection("Jwt"); //appsettings.json 中的 Jwt 配置节
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Name, username), // ✅ 关键，识别用户身份
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"])),
            signingCredentials: creds
        );

        Console.WriteLine($"Generate Token: {token}[]");
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}