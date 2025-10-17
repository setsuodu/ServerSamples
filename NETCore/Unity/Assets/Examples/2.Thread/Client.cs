using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace ClientThread
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
        private Thread receiveThread;

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

        void Start()
        {
            cts = new CancellationTokenSource();
            ConnectToServer(GameConfigs.SERVER_IP, GameConfigs.SERVER_PORT);
        }

        void OnDestroy()
        {
            Disconnect();
        }

        void Update()
        {
            if (!isConnected) return;

            // 示例：按下空格键发送消息
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SendMessageToServer("Unity client pressed Space!");
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Disconnect();
            }
        }

        void ConnectToServer(string ipAddress, int port)
        {
            try
            {
                client = new TcpClient();
                client.Connect(ipAddress, port);
                stream = client.GetStream();
                isConnected = true;
                OnConnect?.Invoke();
                Debug.Log($"[C] Connected to server: {ipAddress}:{port}"); // 连接成功

                // 启动接收数据线程
                receiveThread = new Thread(() => ReceiveData(cts.Token));
                receiveThread.Start();

                // 发送一条初始消息
                //SendMessageToServer("Hello from Unity Client!");
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
                Debug.LogError($"Connection error: {e.Message}");
                isConnected = false;
            }
        }

        private void ReceiveData(CancellationToken token)
        {
            try
            {
                while (isConnected && !token.IsCancellationRequested)
                {
                    if (stream.DataAvailable)
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
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
                        Thread.Sleep(10);
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
                Debug.LogError($"Socket error in ReceiveData: {se.Message}");
                Disconnect();
            }
            catch (System.IO.IOException ioe)
            {
                OnError?.Invoke(ioe);
                Debug.LogError($"IO error in ReceiveData: {ioe.Message}");
                Disconnect();
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
                Debug.LogError($"Unexpected error in ReceiveData: {e.Message}");
                Disconnect();
            }
        }

        void SendMessageToServer(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                Debug.Log($"[C] Sent to server: {message}");
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
                Debug.LogError($"Send error: {e.Message}");
                Disconnect();
            }
        }

        [ContextMenu("Test Send")]
        void SendMessage()
        {
            SendMessageToServer("Client send hello world test.");
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