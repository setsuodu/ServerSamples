using Microsoft.AspNetCore.SignalR; //ASP.NET Core 8.0 内置了，不需要NuGet获取
using SignalRServer.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 添加 SignalR 服务（内置，无需额外包）
builder.Services.AddSignalR();

var app = builder.Build();
//app.UseHttpsRedirection();

// 配置 Hub 路由
app.MapHub<ChatHub>("/chathub");

app.MapGet("/", () => "SignalR Server Running!");

app.Run();
