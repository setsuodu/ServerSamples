using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;

public class TcpServer
{
    private TcpListener _listener;
    private CancellationTokenSource _cts;
    private readonly ConcurrentDictionary<TcpClient, Task> _clients;
    private bool _isRunning;

    // 定义事件
    public event Action<TcpClient>? OnConnect;
    public event Action<TcpClient, Exception>? OnError;
    public event Action<TcpClient, string>? OnMessage;
    public event Action<TcpClient>? OnDisconnect;

    public TcpServer(string ipAddress, int port)
    {
        _listener = new TcpListener(IPAddress.Parse(ipAddress), port);
        _cts = new CancellationTokenSource();
        _clients = new ConcurrentDictionary<TcpClient, Task>();
        _isRunning = false;
    }

    public async Task StartAsync()
    {
        if (_isRunning)
            return;

        _isRunning = true;
        _listener.Start();

        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                HandleClientAsync(client);
            }
        }
        catch (Exception ex)
        {
            if (!_cts.Token.IsCancellationRequested)
                OnError?.Invoke(null, ex);
        }
    }

    private async void HandleClientAsync(TcpClient client)
    {
        try
        {
            OnConnect?.Invoke(client);
            _clients.TryAdd(client, Task.CompletedTask);

            using (var stream = client.GetStream())
            {
                byte[] buffer = new byte[1024];
                while (client.Connected && !_cts.Token.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, _cts.Token).ConfigureAwait(false);
                    if (bytesRead == 0)
                    {
                        // 客户端断开连接
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    OnMessage?.Invoke(client, message);
                }
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke(client, ex);
        }
        finally
        {
            if (client.Connected)
            {
                client.Close();
            }
            OnDisconnect?.Invoke(client);
            _clients.TryRemove(client, out _);
        }
    }

    public async Task SendMessageAsync(TcpClient client, string message)
    {
        if (client == null || !client.Connected)
            return;

        try
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            var stream = client.GetStream();
            await stream.WriteAsync(buffer, 0, buffer.Length, _cts.Token).ConfigureAwait(false);
            await stream.FlushAsync(_cts.Token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            OnError?.Invoke(client, ex);
        }
    }

    public void Stop()
    {
        if (!_isRunning)
            return;

        _isRunning = false;
        _cts.Cancel();
        _listener.Stop();

        foreach (var client in _clients.Keys)
        {
            client.Close();
        }
        _clients.Clear();
    }
}