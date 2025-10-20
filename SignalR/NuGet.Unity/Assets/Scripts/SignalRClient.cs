using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using UnityEngine;

public class SignalRClient : MonoBehaviour
{
    private HubConnection connection;

    async void Start()
    {
        // 初始化 SignalR 连接
        connection = new HubConnectionBuilder()
            .WithUrl("https://your-signalr-server/hub") // 替换为你的 SignalR 服务器地址
            .Build();

        // 注册消息处理
        connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            Debug.Log($"Message from {user}: {message}");
        });

        // 连接到服务器
        try
        {
            await connection.StartAsync();
            Debug.Log("Connected to SignalR server!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Connection failed: {ex.Message}");
        }
    }

    // 发送消息到服务器
    public async Task SendMessage(string user, string message)
    {
        try
        {
            await connection.InvokeAsync("SendMessage", user, message);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Send failed: {ex.Message}");
        }
    }

    async void OnDestroy()
    {
        // 断开连接
        if (connection != null)
        {
            await connection.StopAsync();
            await connection.DisposeAsync();
        }
    }
}