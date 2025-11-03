// src/LeaderboardService/Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LeaderboardService.Data;
using LeaderboardService.Services; // ← 确保引用

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment.EnvironmentName;
Console.WriteLine($"Leaderboard 环境是: {environment}");

// 1. 数据库
var connectionString = builder.Configuration.GetConnectionString("Default");
Console.WriteLine($"Leaderboard 连接SQL: {connectionString}");
builder.Services.AddDbContext<LeaderboardDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Redis 缓存
string redisConn = string.Empty;
if (environment == "Development")
    redisConn = builder.Configuration["Redis:Connection"] ?? "localhost:6379"; //VS环境
else
    redisConn = builder.Configuration["Redis:Connection"] ?? "msa-redis:6379"; //docker环境
Console.WriteLine($"Redis Connection: {redisConn}");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConn;
    options.InstanceName = "LeaderboardCache:";
});

// 3. 注册 LeaderboardCacheService
builder.Services.AddScoped<LeaderboardCacheService>(); // ← 关键一行！

// 4. JWT
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
    var db = scope.ServiceProvider.GetRequiredService<LeaderboardDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();