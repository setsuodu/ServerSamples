# 若联网游戏框架（Weakly Connected）

- 单机+云同步游戏
- 异步通信（上传记录）
- 离线玩，联网同步成绩
- 小游戏，微服务架构

## 架构
https://grok.com/c/50b3c881-c3c6-4b31-b2ab-0a4506676bd1
https://grok.com/c/296f20c3-1a0d-4b22-80cf-c12af7fd2e0b

- Gateway（纯转发）
- AuthServer
	- 生成 JWT Token
	- 处理用户注册/api/register、登陆/api/login、拉取信息/api/get_userinfo
	- EFCore 访问数据库
- GameServer
	- 接收客户端提交分数 👉 EFCore 存储分数
	- 防作弊（可选）
		- 严格验证（操作回放）
		- 简单验证（通关时间vs全服均值/估算均值）
		- 不验证
- Leaderboard
	- 查询各种榜单
	- EFCore 查询排名
	- 用 Redis 频繁查询
- APIServer/DBServer
	- 微服务架构，让他们都能独立访问SQL，不需要了
- Redis
	- 独立部署，所有服务都有访问权
	- Gateway：缓存路由配置、限流计数
	- AuthServer：缓存用户会话、Token 黑名单
	- GameServer：缓存用户最近分数、临时提交缓冲
	- Leaderboard：缓存 Top N 排行榜（核心性能优化
	
[API Gateway]
     ↓ HTTP
[User Service] ──→ PostgreSQL
[Game Service]  ──→ PostgreSQL
[Leaderboard]   ──→ PostgreSQL

## 部署
当前架构较小，全部部署在一台物理机（4C8G5M）上。