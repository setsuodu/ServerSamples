using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TaskTest : MonoBehaviour
{
    public TcpClient _client;
    private NetworkStream _stream;
    private volatile bool _isConnected;

    async void Start()
    {
        await StartAsync("localhost", 8080);
    }

    public async Task StartAsync(string host, int port)
    {
        try
        {
            _client = new TcpClient();
            Debug.Log($"Connecting to {host}:{port}...");
            await _client.ConnectAsync(host, port);
            _stream = _client.GetStream();
            _isConnected = true;
            Debug.Log("Connected to server.");

            // 启动接收消息的任务
            _ = Task.Run(ReceiveMessagesAsync);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Connection error: {ex.Message}");
            _isConnected = false;
        }
    }

    public async Task SendMessageAsync(string message)
    {
        if (!_isConnected)
        {
            Debug.Log("Not connected to server.");
            return;
        }

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message + "\n");
            await _stream.WriteAsync(data, 0, data.Length);
            Debug.Log($"Sent: {message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Send error: {ex.Message}");
            _isConnected = false;
        }
    }

    private async Task ReceiveMessagesAsync()
    {
        byte[] buffer = new byte[1024];

        try
        {
            while (_isConnected)
            {
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    // 正常关闭：对方优雅地关闭了连接
                    // 服务器调用 NetPeer.Disconnect() 断开了我。
                    Debug.Log("Server disconnected.");
                    _isConnected = false;
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Debug.Log($"Received: {message}");
            }
        }
        //catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
        //{
        //    Debug.LogError("套接字操作被中止: " + ex.Message);
        //    // 报错是【由于线程退出或应用程序请求，已中止 I/O 操作。】，无法从 SocketException 捕获错误。
        //}
        //catch (IOException ex) when (!_isConnected)
        //{
        //    Debug.LogError($"Connection closed intentionally.故意、主动关闭: {ex.Message}"); //故意、主动关闭
        //}
        catch (IOException ex) when (ex.InnerException is SocketException se &&
                             (se.SocketErrorCode == SocketError.OperationAborted)) // 通过操作断开
        {
            Debug.LogError("Connection closed or aborted.通过操作断开");
        }
        catch (IOException ex) when (ex.InnerException is SocketException se &&
                             (se.SocketErrorCode == SocketError.ConnectionAborted ||
                              se.SocketErrorCode == SocketError.ConnectionReset)) // 无法捕获，直接跳到 catch《其他未预期的异常》
        {
            // 连接被关闭（可能是调用了 TcpClient.Close() 或客户端断开）
            Debug.LogError("Connection closed or aborted.");
        }
        catch (ObjectDisposedException)
        {
            // TcpClient 或 NetworkStream 已被释放
            Debug.LogError("Connection disposed.");
        }
        catch (Exception ex)
        {
            // 其他未预期的异常
            Debug.LogError($"Receive error: {ex.Message}");
        }
        finally
        {
            // 确保清理资源
            _client?.Close();
        }

        Cleanup();
    }

    [ContextMenu("SendMessage")]
    public async void SendMessage()
    {
        await SendMessageAsync("hehe");
    }

    [ContextMenu("Disconnect")]
    public void Cleanup()
    {
        _isConnected = false; // 设置标志
        _stream?.Close();
        _client?.Close();
        Debug.Log("Disconnected from server.");
    }
}