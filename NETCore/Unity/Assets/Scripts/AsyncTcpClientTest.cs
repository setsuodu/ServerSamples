using UnityEngine;
using Google.Protobuf;

public class AsyncTcpClientTest : MonoBehaviour
{
    private string serverIp = "127.0.0.1";
    private int serverPort = 8080;
    AsyncTcpClient netClient;

    void Start()
    {
        netClient = new AsyncTcpClient();

        // 注册回调
        netClient.OnConnected += OnConnected;
        netClient.OnDisconnected += OnDisconnected;
        netClient.OnDataReceived += OnDataReceived;
        netClient.OnError += OnError;

        netClient.Connect(serverIp, serverPort);
        
        // 发送测试消息
        Invoke("SendTestMessage", 2f);
        //InvokeRepeating("SendTestMessage", -1, 2f);
    }

    void OnDestroy()
    {
        netClient.Disconnect();
    }

    void OnConnected()
    {
        Debug.Log("Connected to server!");
    }
    void OnDisconnected()
    {
        Debug.Log("Disconnected from server!");
    }
    void OnDataReceived(string data)
    {
        Debug.Log($"Received: {data}");
    }
    void OnError(System.Exception ex)
    {
        Debug.LogError($"Error: {ex.Message}");
    }


    void SendTestMessage()
    {
        Debug.Log("SendTestMessage");
        byte[] data = System.Text.Encoding.UTF8.GetBytes("Hello, Server!");
        netClient.Send(data);
    }
    public void SendMessage()
    {
        var msg = new Tutorial.TheMsg
        {
            Name = "Unity",
            Content = "I send a message to you"
        };
        netClient.Send(msg.ToByteArray());
    }
}
