using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace AsyncTcpServer
{
    // 消息结构
    public class Message
    {
        public int MessageId { get; set; }
        public byte[] Data { get; set; }
    }

    // 客户端连接上下文
    public class ClientContext
    {
        public TcpClient Client { get; set; }
        public NetworkStream Stream { get; set; }
        public string ClientId { get; set; }
    }

    // 事件委托
    public delegate Task OnConnectHandler(ClientContext context);
    public delegate Task OnMessageHandler(ClientContext context, Message message);
    public delegate Task OnDisconnectHandler(ClientContext context);
    public delegate Task OnErrorHandler(ClientContext context, Exception ex);

    public class AsyncTcpServer : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly ConcurrentDictionary<string, ClientContext> _clients;
        private readonly CancellationTokenSource _cts;
        private bool _disposed;

        // 事件
        public event OnConnectHandler OnConnect;
        public event OnMessageHandler OnMessage;
        public event OnDisconnectHandler OnDisconnect;
        public event OnErrorHandler OnError;

        public AsyncTcpServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
            _clients = new ConcurrentDictionary<string, ClientContext>();
            _cts = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            _listener.Start();
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    var clientId = Guid.NewGuid().ToString();
                    var context = new ClientContext
                    {
                        Client = client,
                        Stream = client.GetStream(),
                        ClientId = clientId
                    };

                    if (_clients.TryAdd(clientId, context))
                    {
                        _ = Task.Run(() => HandleClientAsync(context), _cts.Token);
                        await (OnConnect?.Invoke(context) ?? Task.CompletedTask);
                    }
                }
            }
            catch (Exception ex) when (!_cts.Token.IsCancellationRequested)
            {
                await (OnError?.Invoke(null, ex) ?? Task.CompletedTask);
            }
        }

        private async Task HandleClientAsync(ClientContext context)
        {
            try
            {
                using (var stream = context.Stream)
                {
                    while (!_cts.Token.IsCancellationRequested && context.Client.Connected)
                    {
                        // 读取消息号（假设4字节整数）
                        var header = new byte[4];
                        int bytesRead = await stream.ReadAsync(header, 0, header.Length, _cts.Token).ConfigureAwait(false);
                        if (bytesRead != 4)
                        {
                            throw new IOException("Failed to read message header");
                        }

                        int messageId = BitConverter.ToInt32(header, 0);

                        // 读取消息长度（假设4字节整数）
                        var lengthBuffer = new byte[4];
                        bytesRead = await stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length, _cts.Token).ConfigureAwait(false);
                        if (bytesRead != 4)
                        {
                            throw new IOException("Failed to read message length");
                        }

                        int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                        // 读取消息体
                        var messageData = new byte[messageLength];
                        bytesRead = await stream.ReadAsync(messageData, 0, messageLength, _cts.Token).ConfigureAwait(false);
                        if (bytesRead != messageLength)
                        {
                            throw new IOException("Failed to read complete message");
                        }

                        var message = new Message
                        {
                            MessageId = messageId,
                            Data = messageData
                        };

                        // 使用Actor模型处理消息
                        await ProcessMessageAsync(context, message);
                    }
                }
            }
            catch (Exception ex)
            {
                await (OnError?.Invoke(context, ex) ?? Task.CompletedTask);
            }
            finally
            {
                if (_clients.TryRemove(context.ClientId, out _))
                {
                    context.Client.Close();
                    await (OnDisconnect?.Invoke(context) ?? Task.CompletedTask);
                }
            }
        }

        // Actor模型处理消息
        private async Task ProcessMessageAsync(ClientContext context, Message message)
        {
            // 这里实现Actor模型的消息处理
            // 每个客户端有一个独立的Actor处理消息
            try
            {
                await (OnMessage?.Invoke(context, message) ?? Task.CompletedTask);
            }
            catch (Exception ex)
            {
                await (OnError?.Invoke(context, ex) ?? Task.CompletedTask);
            }
        }

        // 发送消息
        public async Task SendAsync(string clientId, Message message)
        {
            if (_clients.TryGetValue(clientId, out var context) && context.Client.Connected)
            {
                try
                {
                    using (var stream = context.Stream)
                    {
                        // 发送消息号
                        var header = BitConverter.GetBytes(message.MessageId);
                        await stream.WriteAsync(header, 0, header.Length, _cts.Token).ConfigureAwait(false);

                        // 发送消息长度
                        var lengthBytes = BitConverter.GetBytes(message.Data.Length);
                        await stream.WriteAsync(lengthBytes, 0, lengthBytes.Length, _cts.Token).ConfigureAwait(false);

                        // 发送消息体
                        await stream.WriteAsync(message.Data, 0, message.Data.Length, _cts.Token).ConfigureAwait(false);
                        await stream.FlushAsync(_cts.Token).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    await (OnError?.Invoke(context, ex) ?? Task.CompletedTask);
                }
            }
        }

        public void Stop()
        {
            _cts.Cancel();
            _listener.Stop();

            foreach (var client in _clients)
            {
                client.Value.Client.Close();
            }
            _clients.Clear();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _cts.Cancel();
                _cts.Dispose();
                _listener.Stop();
                foreach (var client in _clients)
                {
                    client.Value.Client.Dispose();
                }
                _clients.Clear();
                _disposed = true;
            }
        }
    }
}