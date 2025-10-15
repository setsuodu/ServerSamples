using System.Text;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


var listener = new TcpListenerActor("127.0.0.1", 8080);

// 注册事件
listener.OnConnect += async (client, clientId) =>
{
    Console.WriteLine($"Client {clientId} connected from {client.Client.RemoteEndPoint}");
};

listener.OnMessage += async (client, clientId, message) =>
{
    Console.WriteLine($"Message from {clientId}: {message}");
    // 回发消息
    var stream = client.GetStream();
    var response = Encoding.UTF8.GetBytes($"Echo: {message}");
    await stream.WriteAsync(response, 0, response.Length);
};

listener.OnDisconnect += async (client, clientId) =>
{
    Console.WriteLine($"Client {clientId} disconnected");
};

listener.OnError += async (client, clientId, ex) =>
{
    Console.WriteLine($"Error for client {clientId}: {ex.Message}");
};

// 启动监听
await listener.StartAsync();