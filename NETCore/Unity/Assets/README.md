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
