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
| 项目  | 操作 | 调试目标 |
|-------|-----|--------|
| ApiGateway     | 开始 | 可以空白 |
| ProductService | 开始 | 可以空白 |
| OrderService   | 开始 | 可以空白 |
	- 新建配置文件👉重命名为DebugAll👉确定
4. F5启动，测试请求
http://localhost:5000/products
http://localhost:5000/orders


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


## 外部访问问题！
方法1. Dockerfile添加（跨环境，推荐）
```
..
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
```
方法2. Programs.cs添加（不兼容vs调试，不推荐）
```
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // 容器里必须监听任意 IP（0.0.0.0），否则外部访问不到
    serverOptions.ListenAnyIP(80);
});
```


## 多个ocelot配置
Program.cs
```C#
var environment = builder.Environment.EnvironmentName;
builder.Configuration.AddJsonFile($"ocelot.{environment}.json", optional: false, reloadOnChange: true);
//ocelot.Docker.json
//ocelot.Development.json
```
launchSettings.json
```
  "environmentVariables": {
	"ASPNETCORE_ENVIRONMENT": "Docker"
  },
```
Dockerfile
```
COPY ocelot.Docker.json .
```


## 问题记录
1. Docker运行后，无法请求api
	- docker中内部网络循环未对外，在Dockerfile中设置
	```
	ENV ASPNETCORE_URLS=http://+:80
	```
2. Docker运行中检测到环境是Development。
	- 原因1：launchSettings.json的设置在生产环境均无效，要设置在Dockerfile中设置
	```
	ENV ASPNETCORE_ENVIRONMENT=Docker
	```
	- 原因2【重点】：
		- 仅用Dockerfile打包时，Docker认Dockerfile中的环境变量
		- 同时存在Dockerfile 和 docker-compose 时，Docker认 docker-compose 中的环境变量。
		所以即使docker-compose中没有写 environment这一行，默认也被认为没有任何环境变量。
	- 原因3：
		- docker-compose-override.yml，会再最后覆盖一遍。
		- 禁用override模式启动。
		```
		docker compose -f docker-compose.yml up
		```
3. docker-compose up --build 经常提示 port is already allocated（端口占用）
	- Docker Desktop 经常在操作VS时，自动生成镜像和容器
	- 构建前清空一遍Container。