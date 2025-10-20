using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Google.Protobuf;
using UnityEngine;

namespace ClientActor
{
    // 定义事件委托
    public delegate void ConnectedHandler();
    public delegate void DisconnectedHandler();
    public delegate void DataReceivedHandler(int messageId, IMessage message);
    public delegate void ClientErrorHandler(Exception error);

    public class Client : MonoBehaviour
    {
        private TcpClient client;
        private NetworkStream stream;
        private bool isConnected;
        private Task receiveTask;

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
            OnData += (id, msg) => Debug.Log($"Event: Received message ID={(MsgId)id}");
            OnError += (error) => Debug.LogError($"Event: Error: {error.Message}");
        }

        async void Start()
        {
            await ConnectToServerAsync(GameConfigs.SERVER_IP, GameConfigs.SERVER_PORT);
        }

        void OnDestroy()
        {
            // 通常在无法使用 async/await 的场景中使用，例如在同步方法中需要调用异步方法并获取结果。
            // 阻塞线程：GetResult() 会阻塞当前线程，可能导致性能问题或死锁（尤其是在 UI 线程或 ASP.NET 经典环境中）。
            // 不推荐：在可以的情况下，优先使用 async/await，因为它更符合异步编程模型，避免阻塞。
            // 异常处理：需要用 try-catch 捕获 AggregateException
            DisconnectAsync().GetAwaiter().GetResult(); // 阻塞直到任务完成
        }

        async void Update()
        {
            if (!isConnected) return;

            // 示例：按下空格键发送消息
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var cmd = new LoginRequest { Username = "Test1", Password = "123456" };
                await SendMessageAsync((int)MsgId.C2S_LOGIN, cmd);
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                DisconnectAsync().GetAwaiter().GetResult();
            }
        }

        private async Task ConnectToServerAsync(string ipAddress, int port)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(ipAddress, port);
                stream = client.GetStream();
                isConnected = true;
                OnConnect?.Invoke();
                Debug.Log($"Connected to server: {ipAddress}:{port}");

                // 启动异步接收任务
                receiveTask = ReceiveMessagesAsync();
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
                Debug.LogError($"Connection error: {e.Message}");
                isConnected = false;
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            try
            {
                while (isConnected)
                {
                    (int messageId, IMessage message) = await ReadMessageAsync(stream);
                    if (message == null) break; // 客户端断开

                    OnData?.Invoke(messageId, message);
                    await DispatchMessageAsync(messageId, message);
                }
            }
            catch (IOException ex) when (ex.InnerException is SocketException se && (se.SocketErrorCode == SocketError.OperationAborted))
            {
                //Debug.LogError("Connection closed or aborted.");  // 通过操作断开
            }
            catch (SocketException se)
            {
                OnError?.Invoke(se);
                Debug.LogError($"Socket error in ReceiveDataAsync: {se.Message}");
                await DisconnectAsync();
            }
            catch (System.IO.IOException ioe)
            {
                OnError?.Invoke(ioe);
                Debug.LogError($"IO error in ReceiveDataAsync: {ioe.Message}");
                await DisconnectAsync();
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
                Debug.LogError($"Receive error: {e.Message}");
                await DisconnectAsync();
            }
        }

        private async Task<(int, IMessage)> ReadMessageAsync(NetworkStream stream)
        {
            // 读取消息长度（4字节）
            byte[] lengthBuffer = new byte[4];
            int bytesRead = await stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length);
            if (bytesRead != 4) return (0, null); // 客户端断开

            int length = BitConverter.ToInt32(lengthBuffer, 0);

            // 读取消息号（4字节）+ 消息体
            byte[] messageBuffer = new byte[length];
            bytesRead = await stream.ReadAsync(messageBuffer, 0, length);
            if (bytesRead != length) return (0, null);

            using MemoryStream ms = new MemoryStream(messageBuffer);
            int messageId = BitConverter.ToInt32(ms.ToArray(), 0); // 消息号
            byte[] protoData = new byte[length - 4];
            Array.Copy(messageBuffer, 4, protoData, 0, length - 4);

            // 根据消息号解析 Protobuf 消息
            IMessage message = (MsgId)messageId switch
            {
                MsgId.S2C_LOGIN => LoginResponse.Parser.ParseFrom(protoData),
                MsgId.S2C_LOGOUT => LogoutResponse.Parser.ParseFrom(protoData),
                _ => throw new Exception($"Unknown message ID: {messageId}")
            };

            return (messageId, message);
        }

        private async Task DispatchMessageAsync(int messageId, IMessage message)
        {
            switch ((MsgId)messageId)
            {
                case MsgId.S2C_LOGIN:
                    await HandleLoginResponseAsync(message as LoginResponse);
                    break;
                case MsgId.S2C_LOGOUT:
                    await HandleQueryResponseAsync(message as LogoutResponse);
                    break;
                default:
                    throw new Exception($"No handler for message ID: {messageId}");
            }
        }

        private async Task HandleLoginResponseAsync(LoginResponse response)
        {
            Debug.Log($"LoginResponse: Success Code={response.Code}");
            await Task.CompletedTask; // 异步方法占位
        }

        private async Task HandleQueryResponseAsync(LogoutResponse response)
        {
            Debug.Log($"QueryResponse: Result Code={response.Code}");
            await Task.CompletedTask; // 异步方法占位
        }

        public async Task SendMessageAsync(int messageId, IMessage message)
        {
            if (!isConnected) return;

            try
            {
                using MemoryStream ms = new MemoryStream();
                message.WriteTo(ms);
                byte[] protoData = ms.ToArray();
                byte[] messageIdBytes = BitConverter.GetBytes(messageId);
                byte[] lengthBytes = BitConverter.GetBytes(protoData.Length + 4);

                // 消息格式：长度 + 消息号 + 消息体
                byte[] fullMessage = new byte[lengthBytes.Length + messageIdBytes.Length + protoData.Length];
                Array.Copy(lengthBytes, 0, fullMessage, 0, lengthBytes.Length);
                Array.Copy(messageIdBytes, 0, fullMessage, lengthBytes.Length, messageIdBytes.Length);
                Array.Copy(protoData, 0, fullMessage, lengthBytes.Length + messageIdBytes.Length, protoData.Length);

                await stream.WriteAsync(fullMessage, 0, fullMessage.Length);
                Debug.Log($"Sent message {(MsgId)messageId} to server.");
            }
            catch (Exception e)
            {
                OnError?.Invoke(e);
                Debug.LogError($"Send error: {e.Message}");
                await DisconnectAsync();
            }
        }

        private async Task DisconnectAsync()
        {
            if (isConnected)
            {
                isConnected = false;
                stream?.Close();
                client?.Close();
                OnDisconnect?.Invoke();
                Debug.Log("Disconnected from server.");
                //await Task.WhenAll(task1, task2, ...); // 等待所有任务完成
                await Task.CompletedTask; // Task中没有实际的异步操作时用这个
            }
        }
    }
}