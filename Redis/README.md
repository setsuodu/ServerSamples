# 实践

## 一、安装 Redis

### 方法一、 Docker（推荐）
```
docker run -d --name redis-dev -p 6379:6379 redis
```

### 方法二、 Redis for Windows（兼容版）
1. 下载压缩包 https://github.com/microsoftarchive/redis/releases
2. 启动Redis
```
D:
cd D:\Program Files\Redis
redis-server.exe redis.windows.conf
```
启动后默认监听在：6379

3. 使用 Redis CLI
```
D:
cd D:\Program Files\Redis
redis-cli
KEYS player:*
```
显示持久化数据 1) player:123
