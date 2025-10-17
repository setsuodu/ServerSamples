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
