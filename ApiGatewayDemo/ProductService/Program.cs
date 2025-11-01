var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var environment = builder.Environment.EnvironmentName;
Console.WriteLine($"当前环境: {environment}");
var app = builder.Build();
app.UseRouting();
app.MapControllers();
app.Run();