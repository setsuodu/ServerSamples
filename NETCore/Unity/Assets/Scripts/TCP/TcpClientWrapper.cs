using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class TcpClientWrapper //Tcp封装
{
    public TcpClientWrapper()
    {
        _cts = new CancellationTokenSource();
    }

    private TcpClient _tcpClient;
    private NetworkStream _networkStream;
    private CancellationTokenSource _cts;
    private bool _isConnected;

    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action<string> OnMessageReceived;

    public async Task ConnectAsync(string host, int port)
    {
        if (_isConnected) return;

        try
        {
            _tcpClient = new TcpClient();
            await _tcpClient.ConnectAsync(host, port).ConfigureAwait(false);
            _networkStream = _tcpClient.GetStream();
            _isConnected = true;

            OnConnected?.Invoke();
            //Debug.Log("Connected to server");

            // Start receiving messages in the background
            _ = Task.Run(() => ReceiveMessagesAsync(_cts.Token), _cts.Token);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Connection failed: {ex.Message}");
            Disconnect();
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if (!_isConnected) return;

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message + "\n");
            await _networkStream.WriteAsync(data, 0, data.Length, _cts.Token).ConfigureAwait(false);
            Debug.Log($"Sent message: {message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Send failed: {ex.Message}");
            Disconnect();
        }
    }

    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[1024];
        StringBuilder messageBuilder = new StringBuilder();

        try
        {
            Debug.Log($"ReceiveMessagesAsync try...{!cancellationToken.IsCancellationRequested} && {_isConnected}");

            while (!cancellationToken.IsCancellationRequested && _isConnected)
            {
                Debug.Log("AAA");

                int bytesRead = await _networkStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);

                Debug.Log($"卡在这 bytesRead={bytesRead}");

                if (bytesRead == 0)
                {
                    Debug.Log("Server disconnected");
                    Disconnect();
                    return;
                }

                Debug.Log("BBB");

                string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                messageBuilder.Append(received);

                // Process complete messages (assuming newline-separated)
                string[] messages = messageBuilder.ToString().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < messages.Length - 1; i++)
                {
                    OnMessageReceived?.Invoke(messages[i]);
                }

                // 模拟长时间运行的任务
                Debug.Log("Thread is running...");
                Thread.Sleep(1000); // 每秒打印一次

                // Keep the last incomplete message
                messageBuilder.Clear();
                if (!received.EndsWith("\n"))
                {
                    messageBuilder.Append(messages[messages.Length - 1]);
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.Log("Thread cancellation requested!");
                    return; // 退出线程
                }
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Receive operation cancelled"); // 取消操作，正常退出
            
            // 已释放，执行清理
            Disconnect();
        }
        catch (Exception ex)
        {
            //Unable to read data from the transport connection:
            //由于线程退出或应用程序请求，已中止 I/O 操作。
            Debug.LogError($"Receive failed: {ex.Message}");

            // 连接已断开，执行清理
            Disconnect();
        }
        finally
        {
            Debug.Log("finally");
            //Disconnect();
        }
    }

    public void Print()
    {
        Debug.Log($"_cts={_cts.Token.IsCancellationRequested}"); //False
    }
    public void Cancel()
    {
        if (_cts != null)
        {
            _cts.Cancel();
        }
    }
    public void Disconnect()
    {
        if (!_isConnected) return;

        try
        {
            _cts.Cancel(); // 取消异步读取
            _networkStream?.Close(); // 关闭流
            _tcpClient?.Close(); // 关闭客户端
            _isConnected = false;
            OnDisconnected?.Invoke();
            Debug.Log("Disconnected from server");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Disconnect error: {ex.Message}");
        }
        finally
        {
            _cts?.Dispose();
            _networkStream = null;
            _tcpClient = null;
        }
    }
}