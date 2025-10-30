# ApiGatewayDemo

## 步骤
1. VS新建项目
	- ASP.NET Core Web API
	- csproj名：ApiGateway，sln名：ApiGatewayDemo
	- .NET 9.0，HTTPS，Controller，（先不加容器支持，确保多项目调试正常）
2. 模拟电商的产品/订单，做个简单代码，确保能调试请求。
	ApiGatewayDemo.sln
├── ProductService/
│   ├── Controllers/ProductsController.cs
│   ├── Program.cs
│   └── Properties/launchSettings.json (端口: 5001)
├── OrderService/
│   ├── Controllers/OrdersController.cs
│   ├── Program.cs
│   └── Properties/launchSettings.json (端口: 5002)
└── Gateway/
    ├── ocelot.json (路由配置)
    ├── Program.cs
    └── Properties/launchSettings.json (端口: 5000)


## 配置多项目启动
1. 解决方案sln（右键👉属性）或（Alt+Enter）
2. 默认单项目👉改多个启动项目，csproj的【操作】都改【开始】。 【调试目标】可以空白。
3. 重命名，DebugAll，确定。
4. 按 F5，启动调试。 👉启动三个黑框 👉设置成功。
5. postman 请求 gateway 的入口。 http://localhost:5000/products
