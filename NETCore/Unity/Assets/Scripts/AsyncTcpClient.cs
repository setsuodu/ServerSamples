using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Debug = UnityEngine.Debug;

public class AsyncTcpClient
{
    private TcpClient tcpClient;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isRunning;

    // 回调委托
    public Action OnConnected;
    public Action OnDisconnected;
    //public Action<string> OnDataReceived;
    public Action<MsgId, IMessage> OnDataReceived;
    public Action<Exception> OnError;

    // 配置
    private readonly byte[] buffer = new byte[1024];

    /// <summary>
    /// 异步连接到服务器
    /// </summary>
    public async void Connect(string ip, int port)
    {
        try
        {
            tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(ip, port);
            stream = tcpClient.GetStream();
            isRunning = true;

            // 触发连接成功回调
            OnConnected?.Invoke();

            // 启动接收线程
            receiveThread = new Thread(ReceiveMessages);
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }
        catch (Exception ex)
        {
            OnError?.Invoke(ex);
            Disconnect();
        }
    }

    /// <summary>
    /// 断开连接
    /// </summary>
    public void Disconnect()
    {
        Debug.Log($"Disconnect: isRunning={isRunning}"); //True
        if (!isRunning) return;

        isRunning = false;

        // 应该先终止线程，然后再关socket
        if (receiveThread != null && receiveThread.IsAlive)
        {
            //receiveThread.Join(); // 等待线程退出（影响Unity主线程卡死）
            receiveThread.Interrupt(); // 它不会强制终止线程，而是让线程有机会处理中断并优雅退出。
            receiveThread?.Abort(); //乎不推荐在 Unity 中使用 Thread.Abort
            receiveThread = null;
        }
        if (tcpClient != null && tcpClient.Connected)
        {
            tcpClient.Close(); // 可选：优雅关闭
            stream?.Close();
            tcpClient = null;
            stream = null;
        }

        //stream?.Close();
        //tcpClient?.Close();
        //receiveThread?.Interrupt(); //它不会强制终止线程，而是让线程有机会处理中断并优雅退出。
        //receiveThread = null;


        // 应该使用 CancellationToken

        // 触发断开连接回调
        OnDisconnected?.Invoke(); //
    }

    /// <summary>
    /// 发送数据
    /// </summary>
    public async void SendBytes(byte[] data)
    {
        if (!tcpClient?.Connected ?? true) return;

        try
        {
            //byte[] data = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            OnError?.Invoke(ex);
            Disconnect();
        }
    }
    public async void SendProto(int messageId, IMessage protoMessage)
    {
        if (stream == null || !tcpClient.Connected) return;

        try
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // 序列化 Protobuf 消息体
                protoMessage.WriteTo(ms);
                byte[] protoBytes = ms.ToArray();

                // 构造消息：长度 (4 bytes) + 消息号 (4 bytes) + 消息体（PB 变长）
                byte[] lengthBytes = BitConverter.GetBytes(protoBytes.Length + 4); // 消息号占 4 字节
                byte[] idBytes = BitConverter.GetBytes(messageId);
                byte[] message = new byte[4 + 4 + protoBytes.Length];

                Buffer.BlockCopy(lengthBytes, 0, message, 0, 4);
                Buffer.BlockCopy(idBytes, 0, message, 4, 4);
                Buffer.BlockCopy(protoBytes, 0, message, 8, protoBytes.Length);

                // 发送消息
                await stream.WriteAsync(message, 0, message.Length);
                UnityEngine.Debug.Log($"Sent message with ID: {messageId}");
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Send failed: {e.Message}");
        }
    }

    /// <summary>
    /// 接收数据
    /// </summary>
    private void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];
        while (isRunning)
        {
            try
            {
                // 读取消息长度
                byte[] lengthBytes = new byte[4];
                if (ReadFully(stream, lengthBytes, 4) != 4) continue;

                int totalLength = BitConverter.ToInt32(lengthBytes, 0);
                if (totalLength <= 4) continue; // 至少包含消息号

                // 读取消息号
                byte[] idBytes = new byte[4];
                if (ReadFully(stream, idBytes, 4) != 4) continue;

                MsgId messageId = (MsgId)BitConverter.ToInt32(idBytes, 0);

                // 读取消息体
                byte[] protoBytes = new byte[totalLength - 4];
                if (ReadFully(stream, protoBytes, protoBytes.Length) != protoBytes.Length) continue;

                // 根据消息 Id 确定用不同的 ProtoClass 反序列化
                //MyMessage message = MyMessage.Parser.ParseFrom(protoBytes);
                // C# 8.0+ 高级语法糖
                IMessage message = messageId switch
                {
                    MsgId.Login => S2C_Login.Parser.ParseFrom(protoBytes), // 登录请求
                    MsgId.Logout => S2C_Logout.Parser.ParseFrom(protoBytes), // 登出请求
                    MsgId.GetFriendList => S2C_GetFriendList.Parser.ParseFrom(protoBytes), // 获取好友列表请求
                    _ => null, // 未知消息
                };
                OnDataReceived?.Invoke(messageId, message);
                //Debug.Log($"Received message ID: {messageId}, Content: {message.ToString()}");

            }
            catch (Exception e)
            {
                Debug.LogError($"Receive failed: {e.Message}");
                isRunning = false;
            }
        }
    }
    // 确保读取完整数据
    private int ReadFully(NetworkStream stream, byte[] buffer, int length)
    {
        int totalRead = 0;
        while (totalRead < length)
        {
            int read = stream.Read(buffer, totalRead, length - totalRead);
            if (read == 0) return totalRead; // 连接关闭
            totalRead += read;
        }
        return totalRead;
    }
}