// src/LeaderboardService/Program.cs
using LeaderboardService.Data;
using LeaderboardService.Services; // ← 确保引用
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment.EnvironmentName;
Console.WriteLine($"Leaderboard 环境是: {environment}");


// === 1. 两个只读 DbContext ===
var gameDbString = builder.Configuration.GetConnectionString("GameDb");
Console.WriteLine($"Leaderboard 连接 GameDb: {gameDbString}");
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseNpgsql(gameDbString));

var userDbString = builder.Configuration.GetConnectionString("UserDb");
Console.WriteLine($"Leaderboard 连接 UserDb: {userDbString}");
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseNpgsql(userDbString));


// === 2. Redis 缓存 ===
string redisConn = string.Empty;
if (environment == "Development")
    redisConn = builder.Configuration["Redis:Connection"] ?? "localhost:6379"; //VS环境
else
    redisConn = builder.Configuration["Redis:Connection"] ?? "msa-redis:6379"; //docker环境
Console.WriteLine($"Redis Connection: {redisConn}");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConn; // ← 这就是连接。无需手动 new Redis 客户端，IDistributedCache 自动处理连接池、重连、序列化。
    options.InstanceName = "LeaderboardCache:";
});

// === 3. JWT 验证 ===
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

// === 4. 注册缓存服务 ===
builder.Services.AddScoped<LeaderboardCacheService>(); // ← 关键一行！

// === 5. MVC + Swagger ===
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// === Redis 连接测试（启动时执行）===
using (var scope = app.Services.CreateScope())
{
    var cache = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
    try
    {
        await cache.SetStringAsync("health-check", "ok", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
        });
        var value = await cache.GetStringAsync("health-check");
        app.Logger.LogInformation("Redis 连接成功！health-check = {Value}", value);
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Redis 连接失败！请检查 Redis__Connection 配置");
        // 可选：抛异常阻止启动
        // throw;
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
