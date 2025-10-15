using System.Threading;
using Google.Protobuf;
using UnityEngine;

public class TcpClientWrapperTest : MonoBehaviour
{
    private string serverIp = "127.0.0.1";
    private int serverPort = 8080;
    TcpClientWrapper netClient;

    async void Start()
    {
        netClient = new TcpClientWrapper();

        netClient.OnConnected += OnConnected;
        netClient.OnDisconnected += OnDisconnected;
        netClient.OnMessageReceived += OnMessageReceived;
        //netClient.OnError += OnError;

        await netClient.ConnectAsync(serverIp, serverPort);
    }

    void Update()
    {
        // 按下空格键取消线程
        if (Input.GetKeyDown(KeyCode.Space))
        {
            netClient.Cancel();
            Debug.Log("Cancellation requested!");
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            netClient.Print();
        }
    }

    void OnConnected()
    {
        Debug.Log("Connected to server!");
    }
    void OnDisconnected()
    {
        Debug.Log("委托 Disconnected from server!");
    }
    void OnMessageReceived(string data)
    {
        Debug.Log($"Received: {data}");
    }
    void OnDataReceived(MsgId id, IMessage data)
    {
        //Debug.Log($"Received: {data}");
    }
    void OnError(System.Exception ex)
    {
        Debug.LogError($"Error: {ex.Message}");
    }

    [ContextMenu("SendLogin")]
    public async void SendLogin()
    {
        await netClient.SendMessageAsync("Hello, Server!");
    }
    [ContextMenu("Disconnect")]
    public void Disconnect()
    {
        netClient.Disconnect(); //主动断开
    }
}
