// src/ApiGateway/Program.cs
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 总结：三行代码读取所有环境变量
var environment = builder.Environment.EnvironmentName;
Console.WriteLine($"ASPNETCORE_ENVIRONMENT 是: {environment}");
string _jwtKey = builder.Configuration["Jwt:Key"];
Console.WriteLine($"Jwt__Key 是: {_jwtKey}");
string _connStr = builder.Configuration.GetConnectionString("Default");
Console.WriteLine($"ConnectionStrings__Default 是: {_connStr}");


// 1. 添加 Ocelot
builder.Configuration.AddJsonFile($"ocelot.{environment}.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

// 2. 全局 JWT 验证（可选：只在网关验证一次）
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-super-secret-jwt-key-1234567890";
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false; // 开发环境
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "GameLeaderboard",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "GameLeaderboard",
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.FromMinutes(5)
    };
});

var app = builder.Build();

// 启用中间件
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// 使用 Ocelot
app.UseOcelot().Wait();

app.Run();