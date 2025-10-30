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
3. 设置多项目启动
	- 默认是单项目启动，修改sln属性👉多个启动项目(M):
	| 项目           | 操作 | 调试目标 |
	|--------------|----|------|
	| ApiGateway    | 开始 | 可以空白 |
	| ProductService| 开始 | 可以空白 |
	| OrderService  | 开始 | 可以空白 |
	- 新建配置文件👉重命名为DebugAll👉确定

| 项目  | 操作 | 调试目标 |
|-------|-----|--------|
| ApiGateway     | 开始 | 可以空白 |
| ProductService | 开始 | 可以空白 |
| OrderService   | 开始 | 可以空白 |

4. F5启动，测试请求
http://localhost:5000/products
http://localhost:5000/orders

| Name  | Age | City   |
|-------|-----|--------|
| Alice | 25  | New York |
| Bob   | 30  | London   |

## Docker支持

1. 为每个项目添加 Dockerfile 支持
	- 加了Dockerfile的项目，启动配置自动被切换到 Container(Dockerfile)
	- 此时确保安装 Docker Desktop 才能启动。
	- 要继续在vs中用调试，手动将他们都切换回来（http）
	- 此时 👉F5调试 和原来一模一样。
2. 右键添加 docker-compose 支持，编辑。
3. 构建&启动项目的容器。
```
docker-compose up --build
```
//up 启动，并运行docker-compose里面所有服务
//--build 强制重新构建镜像

```
docker-compose up -d --build
```
//-d 后台运行，这样不影响继续在终端输入如：docker ps等其他命令