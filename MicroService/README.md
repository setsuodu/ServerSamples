# Micro Service Architecture / MSA

- 微服务架构，弱联网（Weakly Connected）小游戏
- 离线玩，异步通信（上传记录）同步成绩

## 常见问题
1. relation "__EFMigrationsHistory" already exists
🌟最佳实践：一个微服务 = 一个数据库（或 Schema），避免迁移冲突、耦合。🌟
是的，你遇到的问题 非常典型，在微服务架构中使用多个 DbContext（每个微服务或模块一个项目都有自己的 Migrations）时，调用 Migrate() 出现：
textrelation "__EFMigrationsHistory" already exists
这 不是代码重复，而是多个 DbContext 共享同一个数据库却试图创建同一个迁移历史表。
问题本质
EF Core 默认会为 每个 DbContext 在数据库中插入一条记录到 __EFMigrationsHistory 表中，用来跟踪该 Context 的迁移历史。

| 方案 | 适用场景 | 推荐度 |
|------|--------|-------| 
| 独立数据库 | 生产、微服务标准 | ⭐⭐⭐⭐⭐ |
| PostgreSQL Schema | 共享 DB 但支持 Schema | ⭐⭐⭐⭐ |
| 自定义历史表名 | 临时共享 DB | ⭐⭐⭐ |
| 条件执行 Migrate | 开发环境 | ⭐⭐ |
| EnsureCreated | 原型/测试 | ⭐ |

2. 为什么VS调试不会提示报错？
- 与调试机制有关，并非没有错！
	- VS是一步一步，单独运行每个项目，运行完关闭，再启动下一个服务。
	- Docker是同时启动所有服务，执行Migrate()。

3. container msa-postgres-userdb has no healthcheck configured
- 所有数据库必须加 healthcheck: 避免服务启动顺序错误，让服务等待数据库启动后再运行

## Feature

1. 多项目启动，兼容 Docker / VS调试；

2. CI/CD（Github Actions）现代化集成，同时利好 微服务 和 Unity；

3. docker-compose 启动四个项目 + Redis + pgSQL；

4. Gateway ocelot 路由；
    ```bash
    dotnet add package Ocelot
    ```

5. JWT；

    ```bash
    dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
    dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore //Identity
    ```

6. Redis；
	- Redis一般推荐与API服务放在同一台物理机。除非大型项目需要讲Redis集群分布式部署。
	- 用Docker调试，使用docker-compose一键启动，容器内直接使用容器名访问。
	- docker-compose 启动
   ```yml
   services:
    redis:
    image: redis:latest
       container_name: msa-redis
       ports:
         - "6379:6379"
       volumes:
         - redis-data:/data
       command: redis-server --appendonly yes --requirepass mysecretpassword
   
   volumes:
     redis-data:
   ```
	- 用VS调试，外部访问需要额外配置网络。
	- 启动一个独立容器
   ```bash
   docker run -d --name msa-redis -p 6379:6379 -v redis-data:/data redis:latest --bind 0.0.0.0
   docker logs temp-redis
   # 成功：看到最后 "Ready to accept connections"
   ```

7. PostgreSQL；

    - 命令启动
    ```bash
	docker run -d --name msa-postgres -e POSTGRES_PASSWORD=123456 -p 5432:5432 postgres:latest
    ```

## 环境变量

- __表示嵌套配置，.NET自动读取后面的：
  - ConnectionStrings__Default 👉 Default
  - Jwt__Key👉Key
  - 以此类推
- 环境变量优先级：
  - appsettings.json：低，本地
  - appsettings.{Environment}.json：中，appsettings.Production.json。
  - 环境变量：高，ConnectionStrings__Default=...
  - 命令行参数：最高，dotnet run -- 携带的参数，覆盖低级参数
- 生产环境建议：只用环境变量，不提交 appsettings.json 敏感信息。

```yml
# 所有服务通用
environment:
  - ConnectionStrings__Default=Host=localhost;Database=postgres;Username=postgres;Password=123456
  - Jwt__Key=your-super-secret-jwt-key-1234567890
  - Jwt__Issuer=GameLeaderboard
  - Jwt__Audience=GameLeaderboard
  - ASPNETCORE_ENVIRONMENT=Development/Debug/Production/Docker/Release/..
  - Redis__Connection=msa-redis:6379
```

- 修改了环境变量，Visual Studio 需要重启，不然无法得到新值。

| json           | 开发环境                                         | 生产环境                                       |
| -------------- | ------------------------------------------------ | ---------------------------------------------- |
| appsettings    | 不能删。                                         | 不能删，可以不写配置，起兜底作用，防止程序崩溃 |
| launchSettings | 不能删，VS调试 / dotnet run 用它。但是优先级最高 | 删除，不会打包进生产环境。                     |



## EFCore数据库迁移
- 只要一个项目里有 DbContext + DbSet<T>（即有 Model）→ 就必须执行 dotnet ef
	- Leaderboard 没有，不需要执行
- 开发时：在 UserService 和 GameService 各运行一次
	- dotnet ef migrations add
	- dotnet ef database update
- 部署时：所有服务 Program.cs 加 db.Database.Migrate()
- 以后：模型变更 → 只在对应服务运行 dotnet ef
- 效果：
	- F5 启动 → 自动建表
	- docker-compose up → 自动建表
	- 无需手动 dotnet ef database update
```
# UserService
cd UserService
dotnet ef migrations add InitUser --output-dir Data/Migrations
dotnet ef database update

cd..

# GameService
cd GameService
dotnet ef migrations add InitScore --output-dir Data/Migrations
dotnet ef database update

# LeaderboardService（无需迁移，复用表）
```


## 架构说明
https://grok.com/c/50b3c881-c3c6-4b31-b2ab-0a4506676bd1
https://grok.com/c/296f20c3-1a0d-4b22-80cf-c12af7fd2e0b

- ApiGateway（纯转发）
- UserService
	- 生成 JWT Token
	- 处理用户注册/api/register、登陆/api/login、拉取信息/api/get_userinfo
	- EFCore 访问数据库
- GameService
	- 接收客户端提交分数 👉 EFCore 存储分数
	- 防作弊（可选）
		- 严格验证（操作回放）
		- 简单验证（通关时间vs全服均值/估算均值）
		- 不验证
- LeaderboardService
	- 查询各种榜单
	- EFCore 查询排名
	- 用 Redis 频繁查询
- （APIServer/DBServer）
	- 微服务架构，让他们都能独立访问SQL，不需要了
- Redis
	- 独立部署，所有服务都有访问权
	- ApiGateway：缓存路由配置、限流计数
	- UserService：缓存用户会话、Token 黑名单
	- GameService：缓存用户最近分数、临时提交缓冲
	- Leaderboard：缓存 Top N 排行榜（核心性能优化
	

## 部署
架构较小，全部服务部署在一台物理机（4C/8G/5M）上。