using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TcpClientDemo : MonoBehaviour
{
    public TcpClient _client;
    private NetworkStream _stream;
    private bool _isConnected;

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

        while (_isConnected)
        {
            try
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
            catch (Exception ex)
            {
                // 不是 Unity 独有的问题，.NETCore 也会报。
                // Receive error: Unable to read data from the transport connection:
                // 由于线程退出或应用程序请求，已中止 I/O 操作。

                Debug.LogError($"Receive error: {ex.Message}");
                _isConnected = false;
                break;
            }
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
        _isConnected = false;
        _stream?.Close();
        _client?.Close();
        Debug.Log("Disconnected from server.");
    }
}