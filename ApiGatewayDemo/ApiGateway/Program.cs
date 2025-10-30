using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
builder.Services.AddOcelot(builder.Configuration);
var app = builder.Build();

// 生产环境自动重定向到 HTTPS
//app.UseHttpsRedirection();

app.UseOcelot().Wait();  // 加载 Gateway 中间件
app.Run();
