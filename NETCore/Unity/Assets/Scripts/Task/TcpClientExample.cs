using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

// Tcp[√]|Async[√]|Proto[×]|Actor[×] //已经出现断开问题
public class TcpClientExample : MonoBehaviour
{
    private TcpClient _tcpClient;
    private NetworkStream _stream;
    private CancellationTokenSource _cancellationTokenSource;

    [SerializeField] private string serverIp = "127.0.0.1"; // 服务器 IP
    [SerializeField] private int serverPort = 8080;        // 服务器端口

    void Start()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        ConnectToServerAsync(_cancellationTokenSource.Token);
    }

    async void ConnectToServerAsync(CancellationToken cancellationToken)
    {
        try
        {
            _tcpClient = new TcpClient();
            Debug.Log("尝试连接到服务器...");
            await _tcpClient.ConnectAsync(serverIp, serverPort);

            if (_tcpClient.Connected)
            {
                Debug.Log("已连接到服务器！");
                _stream = _tcpClient.GetStream();

                // 启动接收消息的任务
                Task receiveTask = ReceiveMessagesAsync(cancellationToken);

                await receiveTask;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"连接错误: {ex.Message}"); //Token has been disposed.
        }
    }

    async Task SendMessageAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            if (_stream == null || !_stream.CanWrite)
            {
                Debug.LogError("网络流不可用");
                return;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await _stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
            Debug.Log($"已发送消息: {message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"发送消息失败: {ex.Message}");
        }
    }

    async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        // 这里只执行一次，在 await ReadAsync 中循环接收服务器消息
        try
        {
            byte[] buffer = new byte[1024];
            while (!cancellationToken.IsCancellationRequested) //isRunning
            {
                if (_stream == null || !_stream.CanRead)
                {
                    Debug.LogError("网络流不可用");
                    break;
                }


                // 模拟长时间运行的任务
                //Debug.Log("while 循环");

                // 卡住等待
                int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                // 收到下一条消息，继续走。
                // 走完再走上面的while，再卡住。
                Debug.Log($"bytesRead={bytesRead}");
                if (bytesRead > 0)
                {
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Debug.Log($"收到消息: {receivedMessage}");
                }
                else
                {
                    Debug.Log("服务器断开连接");
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("接收任务被取消");
        }
        catch (Exception ex)
        {
            // 客户端执行 Disconnect 时，一定会走到这里。
            // 所以排除一下主动将 (Token.IsCancel == True) 取消的情况。
            if (_cancellationTokenSource.IsCancellationRequested == true)
                return;

            //接收消息失败: Unable to read data from the transport connection:
            //由于线程退出或应用程序请求，已中止 I/O 操作。
            Debug.LogError($"接收消息失败: {ex.Message}");

            //Debug.LogError($"Source={_cancellationTokenSource != null}"); //True
            //Debug.LogError($"SourceCancel={_cancellationTokenSource.IsCancellationRequested}"); //True
            //Debug.LogError($"Net={_tcpClient?.Connected}"); //False
            //Debug.LogError($"Token={_cancellationTokenSource.Token != null}"); //Token has been disposed.取不到
        }
    }

    void OnDestroy()
    {
        Disconnect();
    }

    [ContextMenu("SendMessage")]
    async void SendMessage() 
    { 
        await SendMessageAsync("Hello, Server!", _cancellationTokenSource.Token); 
    }

    [ContextMenu("Disconnect")]
    public void Disconnect()
    {
        if (_tcpClient == null || !_tcpClient.Connected)
        {
            Debug.Log("网络未连接");
            return;
        }

        Debug.Log("客户端主动断开");

        // 清理资源
        try
        {
            _cancellationTokenSource?.Cancel();
            _stream?.Close();
            _tcpClient?.Close();
        }
        catch (Exception ex)
        {
            Debug.LogError($"ex={ex.ToString()}");
            //The CancellationTokenSource has been disposed.
            // OnDestroy再次走到这里，先判断断开了就不要走进来
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
        }
    }
}