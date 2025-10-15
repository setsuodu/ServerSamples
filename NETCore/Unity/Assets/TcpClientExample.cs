using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

// Tcp[√]|Async[×]|Proto[×]|Actor[×] //C/S都正常
public class TcpClientExample : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private string serverIP = "127.0.0.1"; // 服务器 IP
    private int port = 8080; // 服务器端口

    void Start()
    {
        ConnectToServer();
    }

    void ConnectToServer()
    {
        try
        {
            // 创建 TcpClient 实例
            client = new TcpClient();
            client.Connect(serverIP, port);
            Debug.Log("Connected to server!");

            // 获取网络流
            stream = client.GetStream();

            // 开启一个线程或协程来接收消息
            //StartCoroutine(ReceiveMessages()); //卡死
        }
        catch (Exception e)
        {
            Debug.LogError("Connection error: " + e.Message);
        }
    }

    // 发送消息到服务器
    public void SendMessageToServer(string message)
    {
        if (client == null || !client.Connected)
        {
            Debug.LogError("Not connected to server!");
            return;
        }

        try
        {
            // 将消息转换为字节数组
            byte[] data = Encoding.UTF8.GetBytes(message);
            // 发送数据
            stream.Write(data, 0, data.Length);
            Debug.Log("Sent: " + message);
        }
        catch (Exception e)
        {
            Debug.LogError("Send error: " + e.Message);
        }
    }

    // 接收服务器消息
    private System.Collections.IEnumerator ReceiveMessages()
    {
        byte[] buffer = new byte[1024];
        while (true)
        {
            if (client != null && client.Connected)
            {
                try
                {
                    // 读取数据
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Debug.Log("Received: " + message);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Receive error: " + e.Message);
                    break;
                }
            }
            yield return null; // 等待下一帧
        }
    }

    // 在 Unity 界面中测试发送消息
    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 30), "Send Test Message"))
        {
            SendMessageToServer("Hello, Server!");
        }
    }

    // 脚本销毁时清理资源
    void OnDestroy()
    {
        stream?.Close();
        client?.Close();
        Debug.Log("Disconnected from server.");
    }
}