// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


// 使用示例
TcpServer server = new TcpServer("127.0.0.1", 8080);

// 注册事件
server.OnConnect += (client) => Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
server.OnDisconnect += (client) => Console.WriteLine($"Client disconnected: {client.Client.RemoteEndPoint}"); //此时client不是null，但Dispose了，无法access
server.OnMessage += (client, message) => Console.WriteLine($"Received message from {client.Client.RemoteEndPoint}");
server.OnError += (client, ex) => Console.WriteLine($"Error: {ex.Message}");

// 启动服务器
await server.StartAsync();
