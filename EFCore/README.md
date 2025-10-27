# asp.net core 用 EFCore 连接 postgresql

## 环境搭建

1. 下载并安装pgsql到Windows
https://www.enterprisedb.com/downloads/postgres-postgresql-downloads
2. 配置安装路径到环境变量 Path
```
D:\Program Files\PostgreSQL\18\bin
```
3. CMD检查，确保配置正确
```
psql --version
> psql (PostgreSQL) 18.0
```

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
	b. 生成迁移文件：生成到vs工程的 /Migrations 文件夹中
	```
	//dotnet ef 命令，必须确保安装 EntityFrameworkCore.Design包
	dotnet ef migrations add InitialCreate
	```
	c. 应用迁移到数据库：
	```
	//必须appsettings中确保数据库连接正确
	dotnet ef database update
	```
7. 增删改模型后，Migrations文件夹要更新
	a. 添加新模型 Category.cs。 AppDbContext.cs 也要中添加新的DbSet
	```
	public DbSet<Category> Categories { get; set; }
	```
	b. 生产新的迁移文件
	```
	dotnet ef migrations add UpdateModel
	// UpdateModel 是你给这次迁移起的名称，建议用描述性的名称（如 AddCategoryTable 或 AddCategoryIdToProduct）。
	// EF Core 会比较当前的模型（基于 DbContext 和实体类）与 Migrations 文件夹中的最新模型快照（AppDbContextModelSnapshot.cs），然后生成一个新的迁移文件，包含增量更改的逻辑。
	```
	c. 检查生成的迁移文件
	//命令会在 Migrations 文件夹中生成一个新文件，例如 YYYYMMDDHHMMSS_UpdateModel.cs
	d. 应用迁移到数据库
	```
	dotnet ef database update
	```
8. 创建Controller，API控制器。
9. 运行和测试
	```
	dotnet run
	```
	- POST 到 api/Products
	```
	{
		"name": "Sample Product",
		"price": 29.99
	}
	```