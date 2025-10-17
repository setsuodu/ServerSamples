using System.Net;
using System.Net.Sockets;
using Google.Protobuf;

namespace TcpServerActor
{
    class TcpServer
    {
        private TcpListener _listener;
        private bool _isRunning;
        private readonly PlayerManager _playerManager;

        // 事件定义
        public event Action<TcpClient> OnConnect;
        public event Action<TcpClient, UserInfo> OnDisconnect;
        public event Action<TcpClient, UserInfo, int, IMessage> OnData;
        public event Action<TcpClient, Exception> OnError;

        public TcpServer(string ipAddress, int port)
        {
            _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
            _playerManager = new PlayerManager();
        }

        public async Task StartAsync()
        {
            try
            {
                await _playerManager.StartAsync();
                _listener.Start();
                _isRunning = true;
                Console.WriteLine($"Server started on {_listener.LocalEndpoint}");

                while (_isRunning)
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    OnConnect?.Invoke(client);
                    Console.WriteLine($"New client connected: {client.Client.RemoteEndPoint}");

                    // 通过PlayerManager创建PlayerActor
                    PlayerActor player = await _playerManager.CreatePlayerAsync(client);
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke(null, e);
                Console.WriteLine($"Server error: {e.Message}");
            }
            finally
            {
                await StopAsync();
            }
        }

        public async Task StopAsync()
        {
            _isRunning = false;
            _listener?.Stop();
            await _playerManager.StopAsync();
            Console.WriteLine("Server stopped.");
        }
    }
}