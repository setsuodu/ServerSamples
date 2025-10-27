# asp.net core 用 EFCore 连接 postgresql

## Steps

1. 创建ASP.NET Core Web API模板项目， .net 8.0（LTS），https（√）
2. 安装 NuGet包，
```
dotnet add package Microsoft.EntityFrameworkCore //核心包，实现对象关系映射 (ORM)
dotnet add package Microsoft.EntityFrameworkCore.Design //实现设计、数据迁移
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL //EFCore与PgSQL交互
```
3. 配置appsettings.json
4. 创建 模型 & DbContext
5. 配置EFCore服务，Program.cs
```
// 注册 DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```
6. 创建数据库迁移
	a. 安装 EF Core CLI 工具
	```
	dotnet tool install --global dotnet-ef
	```
	b. 生成迁移文件：
	```
	dotnet ef migrations add InitialCreate
	```
	c. 应用迁移到数据库：
	```
	dotnet ef database update
	```
7. 