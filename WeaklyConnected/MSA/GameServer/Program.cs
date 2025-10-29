// src/GameService/Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GameService.Data;

var builder = WebApplication.CreateBuilder(args);

// 数据库
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseNpgsql(connectionString));

// JWT 验证
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-super-secret-jwt-key-1234567890";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "GameLeaderboard";
var audience = builder.Configuration["Jwt:Audience"] ?? "GameLeaderboard";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GameDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();