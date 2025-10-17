Console.WriteLine("Hello, World!");

const string IP = "127.0.0.1";
const int PORT = 8080;

/*{
    // 使用示例
    TcpServer server = new TcpServer(IP, PORT);

    // 注册事件
    server.OnConnect += (client) => Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
    server.OnDisconnect += (client) => Console.WriteLine($"Client disconnected: {client.Client.RemoteEndPoint}"); //此时client不是null，但Dispose了，无法access
    server.OnMessage += (client, message) => Console.WriteLine($"Received message from {client.Client.RemoteEndPoint}");
    server.OnError += (client, ex) => Console.WriteLine($"Error: {ex.Message}");

    // 启动服务器
    await server.StartAsync();
}*/

// 1.Sync
{
    TcpServerSync.TcpServer server = new TcpServerSync.TcpServer(IP, PORT);
    //server.OnConnect += (client) => Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
    //server.OnDisconnect += (client) => Console.WriteLine($"Client disconnected: {client.Client.RemoteEndPoint}"); //此时client不是null，但Dispose了，无法access
    //server.OnMessage += (client, message) => Console.WriteLine($"Received message from {client.Client.RemoteEndPoint}");
    //server.OnError += (client, ex) => Console.WriteLine($"Error: {ex.Message}");
    server.Start();
}