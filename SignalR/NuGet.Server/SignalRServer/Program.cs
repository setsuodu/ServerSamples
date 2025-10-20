using Microsoft.AspNetCore.SignalR; //ASP.NET Core 8.0 内置了，不需要NuGet获取

var builder = WebApplication.CreateBuilder(args);

// 添加 SignalR 服务（内置，无需额外包）
builder.Services.AddSignalR();

var app = builder.Build();

// 配置 Hub 路由
app.MapHub<SignalRServer.Hubs.ChatHub>("/hub");

app.Run();
