using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class AsyncTcpClient
{
    private TcpClient tcpClient;
    private NetworkStream stream;
    private Thread receiveThread;
    private bool isRunning;

    // 回调委托
    public Action OnConnected;
    public Action OnDisconnected;
    public Action<string> OnDataReceived;
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
            receiveThread = new Thread(ReceiveData);
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
        if (!isRunning) return;

        isRunning = false;
        stream?.Close();
        tcpClient?.Close();
        receiveThread?.Interrupt();
        receiveThread = null;

        // 触发断开连接回调
        OnDisconnected?.Invoke();
    }

    /// <summary>
    /// 发送数据
    /// </summary>
    public async void Send(byte[] data)
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

    /// <summary>
    /// 接收数据
    /// </summary>
    private void ReceiveData()
    {
        while (isRunning)
        {
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    // 在主线程中触发接收数据回调
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnDataReceived?.Invoke(message);
                    });
                }
                else
                {
                    // 服务器断开连接
                    Disconnect();
                    break;
                }
            }
            catch (Exception ex)
            {
                if (isRunning)
                {
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        OnError?.Invoke(ex);
                    });
                    Disconnect();
                }
                break;
            }
        }
    }
}