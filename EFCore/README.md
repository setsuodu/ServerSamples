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
	- 生产环境建议使用 User Secrets 或环境变量避免硬编码。
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
	e. 删除 Migrations 文件夹
	```
	dotnet ef migrations remove //用命令，或手动删除
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
	
## Version Control

将 Migrations 文件夹提交到版本控制是强制性的。忽视这一点会给团队协作带来严重问题，并导致数据库架构无法可靠地管理。

正确的流程应该是：
1. 修改数据模型。
2. 运行 dotnet ef migrations add [MigrationName] 创建新的迁移文件。
3. 将新创建的迁移文件提交到 Git。
4. 运行 dotnet ef database update 应用迁移到本地数据库。
5. 在持续集成/持续部署 (CI/CD) 流水线中，自动将迁移应用到生产环境或测试环境。

## 新电脑 Clone 项目后，如何部署？

1. （如本地调试）确保安装 PostgreSQL，并启动服务。
2. 配置连接，appsettings.json。
3. 运行 dotnet ef database update。

## 数据库字符串三种级别

在生产环境中，为避免硬编码连接字符串等敏感信息，推荐使用 User Secrets（开发时）或 环境变量（生产环境）。
以下是具体操作步骤，基于 ASP.NET Core 和 EF Core 连接 PostgreSQL 的场景。

1. appsettings.json（调试环境）
2. User Secrets（开发环境）
	a. 初始化 User Secrets，在项目根目录运行：
	```
	dotnet user-secrets init
	```
	b. 添加连接字符串到 User Secrets
	```
	dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=demodb;Username=postgres;Password=yourpassword"
	```
	c. 代码中访问无需修改代码，ASP.NET Core 会自动从 User Secrets 加载配置，优先级高于 appsettings.json。在 Program.cs 中：
	```
	var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
	```
	d. 注意： User Secrets 仅用于开发环境，不适合生产环境。
	e. 确保 .gitignore 包含 secrets.json。
3. 环境变量（生产环境）
	a. CMD设置环境变量
	```
	 // 临时设置
	set ConnectionStrings__DefaultConnection "Host=localhost;Database=postgres;Username=postgres;Password=123456;"
	 // 永久保存（Windows系统环境变量里，可见添加条目）
	setx ConnectionStrings__DefaultConnection "Host=localhost;Database=postgres;Username=postgres;Password=123456;"
	```
	b. Linux/Mac（终端）：
	```
	export ConnectionStrings__DefaultConnection=Host=localhost;Database=demodb;Username=postgres;Password=yourpassword
	```
	c. Docker用dockerfile或compose配置
	```
	services:
	  app:
		environment:
		  - ConnectionStrings__DefaultConnection=Host=postgres;Database=demodb;Username=postgres;Password=yourpassword
	```
	d. 代码中访问与 User Secrets 相同，ASP.NET Core 的 IConfiguration 会自动从环境变量读取：
	```
	var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
	builder.Services.AddDbContext<AppDbContext>(options =>
		options.UseNpgsql(connectionString));
	```