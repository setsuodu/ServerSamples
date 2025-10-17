using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using Google.Protobuf;

namespace TcpServerProto
{
    // 定义事件委托
    public delegate void ClientConnectedHandler(TcpClient client);
    public delegate void ClientDisconnectedHandler(TcpClient client);
    public delegate void DataReceivedHandler(TcpClient client, int messageId, IMessage message);
    public delegate void ErrorHandler(TcpClient client, Exception error);

    class TcpServer
    {
        private TcpListener listener;
        private bool isRunning;
        private readonly ConcurrentDictionary<TcpClient, Task> clientTasks;

        // 事件定义
        public event ClientConnectedHandler? OnConnect;
        public event ClientDisconnectedHandler? OnDisconnect;
        public event DataReceivedHandler? OnData;
        public event ErrorHandler? OnError;

        public TcpServer(string ipAddress, int port)
        {
            listener = new TcpListener(IPAddress.Parse(ipAddress), port);
            clientTasks = new ConcurrentDictionary<TcpClient, Task>();
        }

        public async Task StartAsync()
        {
            try
            {
                listener.Start();
                isRunning = true;
                Console.WriteLine($"Server started on {listener.LocalEndpoint}"); // 服务器启动成功

                while (isRunning)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    OnConnect?.Invoke(client);
                    Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");

                    // 为每个客户端启动异步处理任务
                    Task clientTask = HandleClientAsync(client);
                    clientTasks.TryAdd(client, clientTask);
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke(null, e);
                Console.WriteLine($"Server error: {e.Message}");
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();

                while (isRunning)
                {
                    // 异步读取消息
                    (int messageId, IMessage message) = await ReadMessageAsync(stream);
                    if (message == null) break; // 客户端断开

                    OnData?.Invoke(client, messageId, message);
                    await DispatchMessageAsync(client, stream, messageId, message);
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke(client, e);
                Console.WriteLine($"Client error: {e.Message}");
            }
            finally
            {
                OnDisconnect?.Invoke(client);
                client.Close();
                clientTasks.TryRemove(client, out _);
                Console.WriteLine($"Client disconnected: {client.Client?.RemoteEndPoint}");
            }
        }

        private async Task<(int, IMessage)> ReadMessageAsync(NetworkStream stream)
        {
            // 读取消息长度（4字节）
            byte[] lengthBuffer = new byte[4];
            int bytesRead = await stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length);
            if (bytesRead != 4) return (0, null); // 客户端断开

            int length = BitConverter.ToInt32(lengthBuffer, 0);

            // 读取消息号（4字节）+ 消息体
            byte[] messageBuffer = new byte[length];
            bytesRead = await stream.ReadAsync(messageBuffer, 0, length);
            if (bytesRead != length) return (0, null);

            using MemoryStream ms = new MemoryStream(messageBuffer);
            int messageId = BitConverter.ToInt32(ms.ToArray(), 0); // 消息号
            byte[] protoData = new byte[length - 4];
            Array.Copy(messageBuffer, 4, protoData, 0, length - 4);

            // 根据消息号解析 Protobuf 消息
            IMessage message = (MsgId)messageId switch
            {
                MsgId.C2SLogin => LoginRequest.Parser.ParseFrom(protoData),
                MsgId.C2SLogout => EmptyRequest.Parser.ParseFrom(protoData),
                MsgId.C2SGetFriendList => FriendListRequest.Parser.ParseFrom(protoData),
                _ => throw new Exception($"Unknown message ID: {messageId}")
            };

            return (messageId, message);
        }

        private async Task DispatchMessageAsync(TcpClient client, NetworkStream stream, int messageId, IMessage message)
        {
            switch ((MsgId)messageId)
            {
                case MsgId.C2SLogin:
                    await HandleLoginAsync(client, stream, message as LoginRequest);
                    break;
                case MsgId.C2SLogout:
                    await HandleLogoutAsync(client, stream, message as EmptyRequest);
                    break;
                default:
                    throw new Exception($"No handler for message ID: {messageId}");
            }
        }

        private async Task SendMessageAsync(NetworkStream stream, int messageId, IMessage message)
        {
            using MemoryStream ms = new MemoryStream();
            message.WriteTo(ms);
            byte[] protoData = ms.ToArray();
            byte[] messageIdBytes = BitConverter.GetBytes(messageId);
            byte[] lengthBytes = BitConverter.GetBytes(protoData.Length + 4);

            // 消息格式：长度 + 消息号 + 消息体
            byte[] fullMessage = new byte[lengthBytes.Length + messageIdBytes.Length + protoData.Length];
            Array.Copy(lengthBytes, 0, fullMessage, 0, lengthBytes.Length);
            Array.Copy(messageIdBytes, 0, fullMessage, lengthBytes.Length, messageIdBytes.Length);
            Array.Copy(protoData, 0, fullMessage, lengthBytes.Length + messageIdBytes.Length, protoData.Length);

            await stream.WriteAsync(fullMessage, 0, fullMessage.Length);
            Console.WriteLine($"Sent message ID {messageId} to client.");
        }

        public async Task StopAsync()
        {
            if (!isRunning) return;

            isRunning = false;
            listener?.Stop();
            await Task.WhenAll(clientTasks.Values);
            Console.WriteLine("Server stopped.");
        }


        #region 消息函数

        private async Task HandleLoginAsync(TcpClient client, NetworkStream stream, LoginRequest request)
        {
            Console.WriteLine($"Handling LoginRequest: Username={request.Username}, Password={request.Password}");

            // 模拟登录逻辑
            var response = new LoginResponse
            {
                Code = 0
            };

            await SendMessageAsync(stream, (int)MsgId.S2CLoginResult, response);
        }

        private async Task HandleLogoutAsync(TcpClient client, NetworkStream stream, EmptyRequest request)
        {
            Console.WriteLine($"Handling LogoutRequest");

            // 模拟查询逻辑
            var response = new LogoutResponse { Code = 0 };

            await SendMessageAsync(stream, (int)MsgId.S2CLogoutResult, response);
        }

        #endregion
    }
}