var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    serverOptions.ListenAnyIP(80); // 👈 关键，允许所有 IP 访问
//});
var environment = builder.Environment.EnvironmentName;
Console.WriteLine($"当前环境: {environment}");
var app = builder.Build();
app.UseRouting();
app.MapControllers();
app.Run();