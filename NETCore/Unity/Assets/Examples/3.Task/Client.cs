using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ClientTask
{
    // 定义事件委托
    public delegate void ConnectedHandler();
    public delegate void DisconnectedHandler();
    public delegate void DataReceivedHandler(string data);
    public delegate void ClientErrorHandler(Exception error);

    public class Client : MonoBehaviour
    {
        private TcpClient client;
        private NetworkStream stream;
        private bool isConnected;
        private CancellationTokenSource cts;

        // 事件定义
        public event ConnectedHandler OnConnect;
        public event DisconnectedHandler OnDisconnect;
        public event DataReceivedHandler OnData;
        public event ClientErrorHandler OnError;

        // 示例：订阅事件
        void Awake()
        {
            OnConnect += () => Debug.Log("Event: Connected to server.");
            OnDisconnect += () => Debug.Log("Event: Disconnected from server.");
            OnData += (data) => Debug.Log($"Event: Received data: {data}");
            OnError += (error) => Debug.LogError($"Event: Error: {error.Message}");
        }

        async void Start()
        {
            cts = new CancellationTokenSource();
            await ConnectToServerAsync(GameConfigs.SERVER_IP, GameConfigs.SERVER_PORT);
        }

        void OnDestroy()
        {
            Disconnect();
        }

        async void Update()
        {
            if (!isConnected) return;

            // 示例：按下空格键发送消息
            if (Input.GetKeyDown(KeyCode.Space))
            {
                await SendMessageToServerAsync("Unity client pressed Space!");
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Disconnect();
            }
        }

        async Task ConnectToServerAsync(string ipAddress, int port)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(ipAddress, port).ConfigureAwait(false);
                stream = client.GetStream();
                isConnected = true;
                OnConnect?.Invoke();
                Debug.Log($"Connected to server: {ipAddress}:{port}");

                // 启动接收数据任务
                _ = Task.Run(() => ReceiveDataAsync(cts.Token), cts.Token);

                // 发送一条初始消息
                await SendMessageToServerAsync("Hello from Unity Client!");
            }
            catch (SocketException se)
            {
                OnError?.Invoke(se);
                Debug.LogError($"Socket error during connection: {se.Message}");
                isConnected = false;
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
                Debug.LogError($"Unexpected error during connection: {e.Message}");
                isConnected = false;
            }
        }

        private async Task ReceiveDataAsync(CancellationToken token)
        {
            try
            {
                while (isConnected && !token.IsCancellationRequested)
                {
                    if (stream.DataAvailable)
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length); // 同步读取
                        if (bytesRead == 0) // 服务器关闭连接
                        {
                            Disconnect();
                            return;
                        }
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        OnData?.Invoke(message);
                        Debug.Log($"Received from server: {message}");
                    }
                    else
                    {
                        // 避免高CPU占用
                        await Task.Delay(10, token);
                    }
                }
            }
            catch (IOException ex) when (ex.InnerException is SocketException se && (se.SocketErrorCode == SocketError.OperationAborted))
            {
                Debug.LogError("Connection closed or aborted."); // 通过操作断开
            }
            catch (SocketException se)
            {
                OnError?.Invoke(se);
                Debug.LogError($"Socket error in ReceiveDataAsync: {se.Message}");
                Disconnect();
            }
            catch (System.IO.IOException ioe)
            {
                OnError?.Invoke(ioe);
                Debug.LogError($"IO error in ReceiveDataAsync: {ioe.Message}");
                Disconnect();
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
                Debug.LogError($"Unexpected error in ReceiveDataAsync: {e.Message}");
                Disconnect();
            }
        }

        async Task SendMessageToServerAsync(string message)
        {
            if (!isConnected) return;

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length, cts.Token).ConfigureAwait(false);
                Debug.Log($"Sent to server: {message}");
            }
            catch (SocketException se)
            {
                OnError?.Invoke(se);
                Debug.LogError($"Socket error while sending: {se.Message}");
                Disconnect();
            }
            catch (System.IO.IOException ioe)
            {
                OnError?.Invoke(ioe);
                Debug.LogError($"IO error while sending: {ioe.Message}");
                Disconnect();
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
                Debug.LogError($"Unexpected error while sending: {e.Message}");
                Disconnect();
            }
        }

        [ContextMenu("Test Send")]
        async void SendMessage()
        {
            await SendMessageToServerAsync("Client send hello world test.");
        }

        [ContextMenu("Test Disconnect")]
        void Disconnect()
        {
            if (!isConnected) return;

            try
            {
                isConnected = false;
                cts?.Cancel();
                stream?.Close();
                client?.Close();
                OnDisconnect?.Invoke();
                Debug.Log("Disconnected from server.");
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
                Debug.LogError($"Error during disconnection: {e.Message}");
            }
            finally
            {
                cts?.Dispose();
                cts = null;
            }
        }
    }
}