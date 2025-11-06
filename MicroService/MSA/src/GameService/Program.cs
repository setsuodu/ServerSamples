// src/GameService/Program.cs
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GameService.Data;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment.EnvironmentName;
Console.WriteLine($"GameService 环境是: {environment}");

// 数据库
var gameDbString = builder.Configuration.GetConnectionString("GameDb");
Console.WriteLine($"GameService 连接 GameDb: {gameDbString}");
builder.Services.AddDbContext<GameDbContext>(options =>
    options.UseNpgsql(gameDbString));

// JWT 验证
var jwtKey = builder.Configuration["Jwt:Key"]
             ?? throw new InvalidOperationException("Jwt:Key 配置缺失！请检查环境变量或 appsettings.json");
var issuer = builder.Configuration["Jwt:Issuer"]
             ?? throw new InvalidOperationException("Jwt:Issuer 配置缺失！");
var audience = builder.Configuration["Jwt:Audience"]
             ?? throw new InvalidOperationException("Jwt:Audience 配置缺失！");

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
    Console.WriteLine("Game数据迁移");
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();