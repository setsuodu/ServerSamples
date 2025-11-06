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


// 1. 添加 Ocelot
builder.Configuration.AddJsonFile($"ocelot.{environment}.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);

// 2. 全局 JWT 验证（可选：只在网关验证一次）
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

var app = builder.Build();

app.UseHttpsRedirection();

// 1. 解析身份
app.UseAuthentication();

// 2. 检查权限
app.UseAuthorization();

// 3. 路由 / 控制器
//app.MapControllers();

// 4. Ocelot（如果使用网关）
await app.UseOcelot();   // 必须在 Authentication 之后

app.Run();