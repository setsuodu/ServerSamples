using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Collections.Concurrent;

// Tcp[√]|Async[√]|Proto[×]|Actor[×]
public class TcpServer
{
    private TcpListener _listener;
    private CancellationTokenSource _cts;
    private readonly ConcurrentDictionary<TcpClient, Task> _clients;
    private bool _isRunning;

    // 定义事件
    public event Action<TcpClient>? OnConnect;
    public event Action<TcpClient, Exception>? OnError;
    public event Action<TcpClient, byte[]>? OnMessage;
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

    // Handle收到的消息，包括Socket的状态Connected、Disconnected等
    // TODO: 怎么分辨？
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
                        OnDisconnect?.Invoke(client); //客户端发送Disconnect，在这里收到
                        break;
                    }

                    OnMessage?.Invoke(client, buffer);
                    string testMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"receive: {testMessage}");

                    // Echo to client
                    await SendMessageAsync(client, $"server receive: {testMessage} is Ok");
                }
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke(client, ex);
        }
        finally
        {
            //Console.WriteLine($"Connected: {client.Connected}"); //False
            //Console.WriteLine($"Null: {client == null}"); //False
            //OnDisconnect?.Invoke(client); //不能在这里处理，已经无法access
            client.Close();
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