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
        //Invoke("SendLogin", 2f);
        //InvokeRepeating("SendTestMessage", -1, 2f);
    }

    void OnDestroy()
    {
        netClient.Disconnect(); //主动断开
    }

    void OnConnected()
    {
        Debug.Log("Connected to server!");
    }
    void OnDisconnected()
    {
        Debug.Log("Disconnected from server!");
    }
    void OnDataReceived(MsgId id, IMessage data)
    {
        //Debug.Log($"Received: {data}");
    }
    void OnError(System.Exception ex)
    {
        Debug.LogError($"Error: {ex.Message}");
    }


    void SendTestMessage()
    {
        Debug.Log("SendTestMessage");
        byte[] data = System.Text.Encoding.UTF8.GetBytes("Hello, Server!");
        netClient.SendBytes(data);
    }
    [ContextMenu("SendLogin")]
    public void SendLogin()
    {
        // 封装消息：在发送前，将消息号和序列化后的 Protobuf 消息体打包成一个完整的字节数组。
        // 拆解消息：在接收时，先读取包总长度，再读取消息号，最后将剩余的字节反序列化为对应的 Protobuf 消息。
        // 异步收发：为避免阻塞主线程，使用异步方法（如 Task）或多线程处理网络通信。
        var msg = new C2S_Login
        {
            Username = "Unity",
            Password = "123456",
        };
        netClient.SendProto((int)MsgId.Login, msg);
    }
    [ContextMenu("Disconnect")]
    public void Disconnect()
    {
        netClient.Disconnect(); //主动断开
    }
}
