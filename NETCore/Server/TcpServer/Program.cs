const string IP = "127.0.0.1";
const int PORT = 8080;

// 1.Sync
/*
{
    TcpServerSync.TcpServer server = new TcpServerSync.TcpServer(IP, PORT);

    // 示例：订阅事件
    server.OnConnect += (client) => Console.WriteLine($"Event: Client {client.Client.RemoteEndPoint} connected.");
    server.OnDisconnect += (client) => Console.WriteLine($"Event: Client {client.Client?.RemoteEndPoint} disconnected.");
    server.OnData += (client, data) => Console.WriteLine($"Event: Data from {client.Client.RemoteEndPoint}: {data}");
    server.OnError += (client, error) => Console.WriteLine($"Event: Error {(client != null ? $"from {client.Client.RemoteEndPoint}" : "on server")}: {error.Message}");
    
    server.Start();
}*/
// 2.Thread
/*
{
    TcpServerThread.TcpServer server = new TcpServerThread.TcpServer(IP, PORT);

    // 示例：订阅事件
    server.OnConnect += (client) => Console.WriteLine($"Event: Client {client.Client.RemoteEndPoint} connected.");
    server.OnDisconnect += (client) => Console.WriteLine($"Event: Client {client.Client?.RemoteEndPoint} disconnected.");
    server.OnData += (client, data) => Console.WriteLine($"Event: Data from {client.Client.RemoteEndPoint}: {data}");
    server.OnError += (client, error) => Console.WriteLine($"Event: Error {(client != null ? $"from {client.Client.RemoteEndPoint}" : "on server")}: {error.Message}");

    server.Start();
}*/
// 3.Task
/*
{
    TcpServerTask.TcpServer server = new TcpServerTask.TcpServer(IP, PORT);

    // 示例：订阅事件
    server.OnConnect += (client) => Console.WriteLine($"Event: Client {client.Client.RemoteEndPoint} connected.");
    server.OnDisconnect += (client) => Console.WriteLine($"Event: Client {client.Client?.RemoteEndPoint} disconnected.");
    server.OnData += (client, data) => Console.WriteLine($"Event: Data from {client.Client.RemoteEndPoint}: {data}");
    server.OnError += (client, error) => Console.WriteLine($"Event: Error {(client != null ? $"from {client.Client.RemoteEndPoint}" : "on server")}: {error.Message}");

    await server.StartAsync();
}*/
// 4.Protobuf
/*
{
    TcpServerProto.TcpServer server = new TcpServerProto.TcpServer(IP, PORT);

    // 订阅事件
    server.OnConnect += (client) => Console.WriteLine($"Event: Client {client.Client.RemoteEndPoint} connected.");
    server.OnDisconnect += (client) => Console.WriteLine($"Event: Client {client.Client?.RemoteEndPoint} disconnected.");
    server.OnData += (client, id, msg) => Console.WriteLine($"Event: Data from {client.Client?.RemoteEndPoint}, ID={id}, Message={msg}");
    server.OnError += (client, error) => Console.WriteLine($"Event: Error {(client != null ? $"from {client.Client?.RemoteEndPoint}" : "on server")}: {error.Message}");

    await server.StartAsync();
}*/
// 5.Actor
{
    TcpServerActor.TcpServer server = new TcpServerActor.TcpServer(IP, PORT);

    // 订阅事件
    server.OnConnect += (client) => Console.WriteLine($"Event: Client {client.Client.RemoteEndPoint} connected.");
    server.OnDisconnect += (client, user_info) => Console.WriteLine($"Event: Client {client.Client?.RemoteEndPoint} disconnected.");
    server.OnData += (client, user_info, id, msg) => Console.WriteLine($"Event: Data from {client.Client?.RemoteEndPoint}, ID={id}, Message={msg}");
    server.OnError += (client, error) => Console.WriteLine($"Event: Error {(client != null ? $"from {client.Client?.RemoteEndPoint}" : "on server")}: {error.Message}");

    await server.StartAsync();
}