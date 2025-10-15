// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


var listener = new AsyncTcpServer.AsyncTcpServer(8080);

// 注册事件
listener.OnConnect += async (context) => await Task.Run(() =>
{
    Console.WriteLine($"[OnConnect] Client {context.ClientId} connected from {context.Client.Client.RemoteEndPoint}");
});

listener.OnMessage += async (context, message) => await Task.Run(() =>
{
    Console.WriteLine($"[OnMessage] Message from {context.ClientId}: {(MsgId)message.MessageId}");
    // 回发消息
    //var stream = context.Client.GetStream();
    //var response = Encoding.UTF8.GetBytes($"Echo: {message}");
    //await stream.WriteAsync(response, 0, response.Length);
});

listener.OnDisconnect += async (context) => await Task.Run(() =>
{
    Console.WriteLine($"[OnDisconnect] Client {context.ClientId} disconnected");
});

listener.OnError += async (context, ex) => await Task.Run(() =>
{
    Console.WriteLine($"[OnError] Error for client {context.ClientId}: {ex.Message}");
});

// 启动监听
await listener.StartAsync();