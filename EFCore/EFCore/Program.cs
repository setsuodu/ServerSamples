using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EFCore.Data;
using EFCore.Middleware;
using EFCore.Services;

var builder = WebApplication.CreateBuilder(args);

// 配置
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IFriendService, FriendService>();
// 注册 AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"]!;
var key = Encoding.ASCII.GetBytes(jwtKey);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"]
    };
});

var app = builder.Build();

// 中间件
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 模拟登录（测试用）
//app.MapPost("/api/login", () =>
//{
//    var claims = new[]
//    {
//        new Claim(ClaimTypes.NameIdentifier, "1"),
//        new Claim(ClaimTypes.Name, "testuser"),
//        new Claim(ClaimTypes.Email, "test@example.com")
//    };

//    var tokenHandler = new JwtSecurityTokenHandler();
//    var tokenDescriptor = new SecurityTokenDescriptor
//    {
//        Subject = new ClaimsIdentity(claims),
//        Expires = DateTime.UtcNow.AddHours(1),
//        Issuer = builder.Configuration["Jwt:Issuer"],
//        Audience = builder.Configuration["Jwt:Audience"],
//        SigningCredentials = new SigningCredentials(
//            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]!)),
//            SecurityAlgorithms.HmacSha256Signature)
//    };

//    var token = tokenHandler.CreateToken(tokenDescriptor);
//    var tokenString = tokenHandler.WriteToken(token);

//    return Results.Ok(new { token = tokenString });
//});

Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("123456"));
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("654321"));

app.Run();