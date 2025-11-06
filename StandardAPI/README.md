# Standard API Sample

# API
1. 客户端提交BUG。
[POST] http://localhost:5000/bugs
```json
{
    "id": 3,
    "title": "UI Crash",
    "description": "Click button causes freeze",
    "createdAt": "2025-11-04T01:58:13.2799339Z"
}
```
2. 查询提交的BUG。
[GET] http://localhost:5000/bugs

# Steps
1. 创建项目（src结构）
```
cd ..\ServerSamples\StandardAPI
dotnet new sln -n StandardAPI
mkdir src && cd src

dotnet new webapi -n ApiGateway -o ApiGateway
dotnet new webapi -n WebApi -o WebApi

cd ..
dotnet sln add src/ApiGateway/ApiGateway.csproj
dotnet sln add src/WebApi/WebApi.csproj
```

2. 引入必要的包
	- PostgreSQL:
		- <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.10" />
		- <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.4" />
		- 引用9.0.0似乎就会导致database update报错。(__EF...Histroy 用8.0.8目前正常)
		```
		<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.8"/>
		<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.8" />
		```
		- 全部升级后正常，说明之前bug并不是此造成的。
	- ocelot:
		- <PackageReference Include="Ocelot" Version="24.0.1" />
	- SwaggerUI:
		- <PackageReference Include="Swashbuckle.AspNetCore" Version="9.0.6" />

3. 对于 EFCore
	- Microsoft.EntityFrameworkCore.Design 是必须引用的库。
	- Microsoft.EntityFrameworkCore 不需要手动添加，只因 Npgsql.EntityFrameworkCore.PostgreSQL 的依赖库里包含了，安装后可以手动查看。
	- 执行数据迁移命令，可以有2中方式：
		1. 项目中引入 Microsoft.EntityFrameworkCore.Tools（不推荐放项目里）
		2. .NET CLI 工具，全局安裝（推荐）
		```
		dotnet tool install --global dotnet-ef
		dotnet tool update --global dotnet-ef // 來更新
		```

4. 创建项目代码
```
BugTracker/
├─ .dockerignore
├─ docker-compose.yml
├─ (Dockerfile.apigateway) // 也可以选择放在这里，构建context上下文目录一致性
├─ (Dockerfile.webapi) // 同👆
└─ src/
   ├─ ApiGateway/
   │   ├─ ApiGateway.csproj
   │   ├─ Program.cs
   │   ├─ ocelot.json
   │   └─ Dockerfile
   └─ WebApi/
       ├─ WebApi.csproj
       ├─ Program.cs
       ├─ Controllers/
       │   └─ BugsController.cs
       ├─ Models/
       │   ├─ Bug.cs
       │   └─ BugDto.cs
       ├─ Data/
       │   └─ AppDbContext.cs
       ├─ Services/
       │   └─ IBugService.cs
       │   └─ BugService.cs
       └─ Dockerfile
```

5. 编写Dockerfile, docker-compose.yml
	- 默认生成的上下文关系、路径都是错的，需要手动修改。
	- Dockerfile 紧跟 docker-compose 里的 context 路径
	```
	webapi:
    build:
      context: ./src/WebApi // ①👉已经指向WebAPI目录
      dockerfile: Dockerfile
	```
	则，Dockerfile 中默认的👇：
	```
	COPY ["src/WebApi/WebApi.csproj", "WebApi/"] ❌已经在WebAPI目录了，不存在src/...
	```
	修改为：
	```
	COPY *.csproj ./
	```
> Dockerfile 常用路径说明（通常在 COPY / ADD 时用）
> ../ 👉 Dockerfile 中不存在
> ./ 👉 当前目录
> . 👉 来源（context）或目标（WORKDIR）文件夹
	
6. 检查环境变量、覆盖关系
- appsettings.json
- launchsettings.json（用docker时无关）
- docker-compose: environment
- docker-compose-override: environment
- 暴露端口 ASPNETCORE_URLS=http://+:8080

7. 启动容器，测试API

# Commands

单开数据库容器
```
docker-compose up db -d
```
EFCore 数据迁移
```
dotnet ef migrations add InitWebApi // 开发阶段必须至少执行一次，生成Migrations目录，提交Git，他人拉取后不用再执行。
dotnet ef database update
```
项目一键打包&运行
```
docker compose up --build
```
