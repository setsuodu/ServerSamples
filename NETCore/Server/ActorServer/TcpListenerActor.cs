using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

public class TcpListenerActor
{
    private readonly TcpListener _listener;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ConcurrentDictionary<Guid, TcpClient> _clients;
    private readonly ActorSystem _actorSystem;

    // 定义事件委托
    public delegate Task OnConnectHandler(TcpClient client, Guid clientId);
    public delegate Task OnMessageHandler(TcpClient client, Guid clientId, string message);
    public delegate Task OnDisconnectHandler(TcpClient client, Guid clientId);
    public delegate Task OnErrorHandler(TcpClient client, Guid clientId, Exception ex);

    // 事件
    public event OnConnectHandler? OnConnect;
    public event OnMessageHandler? OnMessage;
    public event OnDisconnectHandler? OnDisconnect;
    public event OnErrorHandler? OnError;

    public TcpListenerActor(string ipAddress, int port)
    {
        _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
        _cancellationTokenSource = new CancellationTokenSource();
        _clients = new ConcurrentDictionary<Guid, TcpClient>();
        _actorSystem = new ActorSystem();
    }

    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine($"TcpListener started on {_listener.LocalEndpoint}");

        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync();
                var clientId = Guid.NewGuid();
                _clients.TryAdd(clientId, client);

                // 创建Actor处理客户端
                var clientActor = _actorSystem.CreateActor<ClientActor>(clientId.ToString());
                await clientActor.Tell(new ClientConnected(client, clientId));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Listener error: {ex.Message}");
        }
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
        foreach (var client in _clients.Values)
        {
            client.Close();
        }
        _listener.Stop();
    }

    // Actor系统
    private class ActorSystem
    {
        private readonly ConcurrentDictionary<string, IActor> _actors;

        public ActorSystem()
        {
            _actors = new ConcurrentDictionary<string, IActor>();
        }

        public T CreateActor<T>(string actorId) where T : IActor, new()
        {
            var actor = new T();
            _actors.TryAdd(actorId, actor);
            return actor;
        }
    }

    private interface IActor
    {
        Task Tell(object message);
    }

    // 客户端Actor
    private class ClientActor : IActor
    {
        private TcpClient _client;
        private Guid _clientId;
        private readonly TcpListenerActor _parent;

        public ClientActor()
        {
            _parent = null; // 需要通过构造函数注入
        }

        public async Task Tell(object message)
        {
            if (message is ClientConnected connected)
            {
                _client = connected.Client;
                _clientId = connected.ClientId;

                await (_parent?.OnConnect?.Invoke(_client, _clientId) ?? Task.CompletedTask);
                await HandleClientAsync();
            }
        }

        private async Task HandleClientAsync()
        {
            try
            {
                var stream = _client.GetStream();
                var buffer = new byte[1024];

                while (_client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        await (_parent?.OnDisconnect?.Invoke(_client, _clientId) ?? Task.CompletedTask);
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    await (_parent?.OnMessage?.Invoke(_client, _clientId, message) ?? Task.CompletedTask);
                }
            }
            catch (Exception ex)
            {
                await (_parent?.OnError?.Invoke(_client, _clientId, ex) ?? Task.CompletedTask);
            }
            finally
            {
                _client.Close();
                _parent?._clients.TryRemove(_clientId, out _);
            }
        }
    }

    // 客户端连接消息
    private class ClientConnected
    {
        public TcpClient Client { get; }
        public Guid ClientId { get; }

        public ClientConnected(TcpClient client, Guid clientId)
        {
            Client = client;
            ClientId = clientId;
        }
    }
}
