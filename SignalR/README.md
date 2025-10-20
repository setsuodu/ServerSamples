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
2. Nuget导入：
	a. Unity
	b. ASP.NET Core