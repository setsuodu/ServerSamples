# TODO:
1. 服务器PlayerManager管理，客户端消息不发UserId，通过Client连接获取UserId。
2. proto 默认生成的 C# 代码：会去掉下划线，且变成驼峰命名。
```
// .proto 文件
enum UserStatus {
  USER_OFFLINE = 0;
  USER_ONLINE = 1;
  USER_AWAY = 2;
}
```
```
// .cs 文件，大写+驼峰
public enum UserStatus
{
    UserOffline = 0,
    UserOnline = 1,
    UserAway = 2
}
```
消息号且不依赖Protobuf，也不涉及前后端跨语言，建议直接在C#中定义。避免proto转换后命名格式变化问题。

# Introduce
Assets/Examples
1. Sync: How to use TcpListener, TcpClient basically. Sync Blcok.
	- Whole error catch.
		- try .. receive message ..
		- catch (IOException ex) when (!_isConnected)
		- catch (IOException ex) when (ex.InnerException is SocketException se &&
                     (se.SocketErrorCode == SocketError.OperationAborted)) // 通过操作断开
		- catch (ObjectDisposedException)
		- catch (Exception ex)
		- finally .. client.Close();
	- Whole net Interface
		- OnConnect/OnDisconnect/OnData/OnError
2. Thread: Async ues Thread, and stop by CancellationToken gracefully.
3. Task: Async use Task, async-await.
	- NetworkStream.Read 仍为阻塞调用，但通过 Task 实现异步运行
4. Protobuf: Import Google.Protobuf, instead of binary.
5. Actor: Bring a simple actor model to manage clients in server logic.
	- PlayerManager就是Actor模型。每个客户端独立处理、跟踪、管理。
	- 客户端也添加 PlayerManager，管理房间队友。

# Test
> Compeletly test Client/Server Operate Connect/Send/Recv/Disconnect, then next..

## Bug History
1. 客户端主动断开，服务端HandleClient..finally..报错，服务端Crash。
```
client.Client null

System.NullReferenceException:“Object reference not set to an instance of an object.”
System.Net.Sockets.TcpClient.Client.get 返回 null。
```
client.Client?.RemoteEndPoint 加问号防止空
2. 在 Unity 中使用 TcpClient 接收网络消息时，通常是在 后台线程 中进行的（例如通过异步读取或 Thread），而 Unity 的大多数 API（比如创建 GameObject、修改 UI、发事件等）只能在主线程中调用。
