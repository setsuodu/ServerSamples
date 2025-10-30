using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
var environment = builder.Environment.EnvironmentName;
builder.Configuration.AddJsonFile($"ocelot.{environment}.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);
//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    serverOptions.ListenAnyIP(80); // 👈 关键，允许所有 IP 访问
//});
Console.WriteLine($"当前环境: {environment}");
var app = builder.Build();

// 生产环境自动重定向到 HTTPS
//app.UseHttpsRedirection();

app.UseOcelot().Wait();  // 加载 Gateway 中间件
app.Run();
