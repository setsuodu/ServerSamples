using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WebApi.Data;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// PostgreSQL
string connStr = builder.Configuration.GetConnectionString("DefaultConnection")?? string.Empty;
Console.WriteLine($"PostgreSQL : {connStr}");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connStr));

// 健康检查
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("PostgreSQL Database")
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "api" });

builder.Services.AddScoped<IBugService, BugService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 健康端点
app.MapHealthChecks("/health", new()
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(report.Status.ToString());
    }
});

// 自动迁移
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    Console.WriteLine("Database migrated successfully!");
}

app.MapControllers();

app.Run();