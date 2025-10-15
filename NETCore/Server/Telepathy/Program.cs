using System.Text;
using Telepathy;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

// 创建服务器实例，最大消息大小为16KB
Server server = new Server(16 * 1024);

// 设置回调
server.OnConnected += (connectionId, message) =>
{
    Console.WriteLine($"Client connected: {connectionId} | {message}");
};

server.OnData += (connectionId, message) =>
{
    string receivedMessage = Encoding.UTF8.GetString(message.Array, message.Offset, message.Count);
    Console.WriteLine($"Received from {connectionId}: {receivedMessage}");
    // Echo the message back to the client
    server.Send(connectionId, message);
};

server.OnDisconnected += (connectionId) =>
{
    Console.WriteLine($"Client disconnected: {connectionId}");
};

// 启动监听
int port = 7777;
server.Start(port);
Console.WriteLine($"[Server] Started on port {port}");

// 循环处理事件
while (true)
{
    server.Tick(100); // 处理连接/断开/消息
    Thread.Sleep(10);  // 避免CPU占用过高
}