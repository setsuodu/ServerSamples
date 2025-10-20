# SignalR for Unity & ASP.NET Core

## 环境设置

1. 手动导入：23个dll（Win/Linux/MacOS）
	- 插件1个
		- [Microsoft.AspNetCore.SignalR.Client 9.0.0] for [.NETStandard 2.0]
	- 依赖22个
		- Microsoft.AspNetCore.Connections.Abstractions
		- Microsoft.AspNetCore.Http.Connections.Client
		- Microsoft.AspNetCore.Http.Connections.Common
		- Microsoft.AspNetCore.SignalR.Client.Core
		- Microsoft.AspNetCore.SignalR.Common
		- Microsoft.Extensions.DependencyInjection.Abstractions
		- Microsoft.Extensions.Options
		- ... See More in [Assets/Plugins].
	- 创建 link.xml，配置动态库引用
2. Nuget导入：24个dll（多一个System.ComponentModel.Annotations.5.0.0）
	a. Unity：
		- NuGet 搜索 [Microsoft.AspNetCore.SignalR.Client]，安装最新版本，如[9.0.10]
	b. 服务器
		- ASP.NET Core 空 / .net 8.0
		- 服务器不需要安装任何关于 SignalR 的包，因为最新的.NET 8.0已经集成了。
			- Microsoft.AspNetCore.SignalR 1.2.0（ASP.NET Core 2.x时代产物，已弃用 deprecated）
			- Microsoft.AspNetCore.SignalR.Client（服务端需要完整的SignalR，包含服务器核心逻辑，处理客户端连接、消息广播等）
		- using Microsoft.AspNetCore.SignalR;  // 这会自动引用内置的 SignalR