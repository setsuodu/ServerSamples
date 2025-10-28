using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EFCore.Data;
using EFCore.DTOs.Auth;
using EFCore.Exceptions;

namespace EFCore.Services
{
    public class AuthService : IAuthService
    {
        private readonly GameDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(GameDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive && !u.IsBanned);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new BusinessException("用户名或密码错误", 401);

            var token = GenerateJwtToken(user);

            return new LoginResponse
            {
                Token = token,
                UserId = user.Id,
                Username = user.Username
            };
        }

        private string GenerateJwtToken(Models.User user)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
