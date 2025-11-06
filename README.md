# ServerSamples

> Host on Windows/Ubuntu/Docker/Kubernetes

# Introduce

## Agones[Framework]
A Google framework with Kubernetes.

## JWT.NET[Tool]
A modern, stateless, efficiency, security way to protect API.
- JWT = JSON Web Token (Single Token).
- JWT + Identity (Double Token).

## MagicOnion[Framework]
A Cygames RPC framework for .NET platform.

## MailServer[Tool]
C# send email to user.

## MongoDB[SQL]
Server side integrate mongodb.

## Nakama[Framework]
A distributed framework with dashboard.

## Orleans[Framework]
Microsoft framework.

## SignalR[Tool]
Communicatie between different servers.

## Steamworks.NET[SDK]
SteamAPI, lobby & matchmaking.

## WebServer[Tool]
In Unity WebServer which could be built as UnityServer.


1. MicroService
- 创建原因：
	- 做一个淘宝小游戏类，通用后端微服务，实现常用API接口。
	- 了解微服务架构。 服务间通信（RESTful），消息处理（MQ）
	- 了解部署使用 Redis，了解使用 PostgreSQL + CFCore数据迁移。
- 实现效果：
	- 每个服务单独创建一个独立DB服务，避免Migrate()执行冲突
		- VS调试是关闭一个服务，才启动下一个。容器是同时启动，有共享资源争抢。
	- 尚存🐞BUG🐞：401 Unauthorized👉Gateway Route无法转发👉很可能是JWT验证失败？
2. ApiGatewayDemo
- 创建起因：
	- 前个项目卡在 VS & Docker 兼容调试很久（虽然调通）。思路清晰的再实践一次，记录操作和容易出现的错误。
	- 前个项目 API 接口一下子设计了太多，还用了SQL。 这里简化为一个 POST + 一个 GET，都是默认组装的假数据。
	- 着重了解🌟路由转发🌟，先忽略其他。
- 实现效果：
	- 了解了 ocolot.json 双环境设置
	- 实现了 VS 切换两个环境，API都能成功调用
	- 初次认识了多个配置影响环境变量
	- 初次实践了 Dockerfile & docker-compose 编写
3. DockerWorkflow
- 创建起因：
	- 之前关注于VS多项目调试，以及与 docker 环境兼容。这里完全放弃VS调试，直接上 docker。专注于熟知 docker 的工作流程。
	- 为了解 CI/CD，尝试了多个平台环境下，远程提交操作
- 实现效果：
	- 了解了 Docker 镜像仓库👉使用环境的优选：
		- Github GHCR: 开源项目
		- DockerHub: 👆上述发布同时推一次
		- Gitlab: Unity AB打包，企业项目
	- 实践了 Github / Dockerhub 推送
	- 实践了 Github Actions 平台配置、触发配置
4. StandardAPI
- 创建起因：
	- Dockerfile 路径始终报错，用空项目分析错误，学习语法。
	- 手动打印运行时环境变量，查找被修改原因
		- appsettings.json
		- launchsettings.json
		- 系统 / 环境变量
		- docker-compose.yml
		- docker-compose-override.yml
- 实现效果：
	- 从创建阶段，就用/src目录
	- 深入理解 docker-compose
		1. services name / image name / container name 区别和作用
		2. build: / context: 上下文，和 Dockerfile 的初始路径关系
		3. depends_on: & healthcheck: 容器启动顺序和依赖
		4. environment: 覆盖优先级
	- 认识了 Dockerfile 多阶段构建，COPY、ARG、FROM、WORKDIR、等作用
	- 写了 Dockerfile 通用模板