using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class SignalRClient : MonoBehaviour
{
    private string serverUrl = "http://localhost:5064/chathub";
    private HubConnection connection;

    async void Start()
    {
        // 初始化 SignalR 连接
        connection = new HubConnectionBuilder()
            .WithUrl(serverUrl)
            .Build();

        // 注册消息处理
        connection.On<string, string>("ReceiveMessage", (user, message) =>
        {
            Debug.Log($"④【{DateTime.Now.ToString("HH:mm:ss.fff")}】Server broadcast {user}: {message}");
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
            Debug.Log($"①【{DateTime.Now.ToString("HH:mm:ss.fff")}】UnityServer RPC 调用 MasterServer/chathub");
            await connection.InvokeAsync("SendMessage", user, message);
            Debug.Log($"⑤【{DateTime.Now.ToString("HH:mm:ss.fff")}】UnityServer RPC 调用结束");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Send failed: {ex.Message}");
        }
    }

    async void Update()
    {
        // Test SendMessage
        if (Input.GetKeyDown(KeyCode.Space))
        {
            await SendMessage("test1", "are you ok?");
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