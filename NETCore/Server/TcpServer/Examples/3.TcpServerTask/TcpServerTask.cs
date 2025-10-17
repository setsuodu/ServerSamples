using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpServerTask
{
    // 定义事件委托
    public delegate void ClientConnectedHandler(TcpClient client);
    public delegate void ClientDisconnectedHandler(TcpClient client);
    public delegate void DataReceivedHandler(TcpClient client, string data);
    public delegate void ErrorHandler(TcpClient client, Exception error);

    class TcpServer
    {
        private TcpListener listener;
        private bool isRunning;
        private CancellationTokenSource cts;

        // 事件定义
        public event ClientConnectedHandler OnConnect;
        public event ClientDisconnectedHandler OnDisconnect;
        public event DataReceivedHandler OnData;
        public event ErrorHandler OnError;

        public TcpServer(string ipAddress, int port)
        {
            listener = new TcpListener(IPAddress.Parse(ipAddress), port);
            cts = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            try
            {
                listener.Start();
                isRunning = true;
                Console.WriteLine($"Server started on {listener.LocalEndpoint}"); // 服务器启动成功

                // 异步接受客户端连接
                await AcceptClientsAsync(cts.Token);
            }
            catch (Exception e)
            {
                OnError?.Invoke(null, e);
                Console.WriteLine($"Server error: {e.Message}");
            }
        }

        private async Task AcceptClientsAsync(CancellationToken token)
        {
            try
            {
                while (isRunning && !token.IsCancellationRequested)
                {
                    try
                    {
                        // 阻塞等待客户端连接
                        TcpClient client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                        OnConnect?.Invoke(client);
                        Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");

                        // 为每个客户端启动新 Task 处理
                        _ = Task.Run(() => HandleClientAsync(client, token), token);
                    }
                    catch (SocketException se) when (se.SocketErrorCode == SocketError.Interrupted)
                    {
                        // 正常中断（服务器关闭）
                        break;
                    }
                    catch (SocketException se)
                    {
                        OnError?.Invoke(null, se);
                        Console.WriteLine($"Socket error accepting client: {se.Message}");
                    }
                    catch (Exception e)
                    {
                        OnError?.Invoke(null, e);
                        Console.WriteLine($"Unexpected error accepting client: {e.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (Exception e)
            {
                OnError?.Invoke(null, e);
                Console.WriteLine($"Unexpected error in AcceptClientsAsync: {e.Message}");
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            NetworkStream stream = null;
            try
            {
                stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead;

                while (!token.IsCancellationRequested)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length); // 同步读取
                    if (bytesRead == 0) // 客户端正常关闭连接
                        break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    OnData?.Invoke(client, message);
                    Console.WriteLine($"Received from client {client.Client.RemoteEndPoint}: {message}");

                    // 回送消息给客户端
                    string response = $"Server received: {message}";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseData, 0, responseData.Length, token).ConfigureAwait(false);
                    Console.WriteLine($"Sent to client: {response}");
                }
            }
            catch (SocketException se)
            {
                OnError?.Invoke(client, se);
                Console.WriteLine($"Socket error for client {client.Client.RemoteEndPoint}: {se.Message}");
            }
            catch (System.IO.IOException ioe)
            {
                OnError?.Invoke(client, ioe);
                Console.WriteLine($"IO error for client {client.Client.RemoteEndPoint}: {ioe.Message}");
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (Exception e)
            {
                OnError?.Invoke(client, e);
                Console.WriteLine($"Unexpected error for client {client.Client.RemoteEndPoint}: {e.Message}");
            }
            finally
            {
                stream?.Close();
                client?.Close();
                OnDisconnect?.Invoke(client);
                Console.WriteLine($"Client disconnected: {client.Client?.RemoteEndPoint}");
            }
        }

        public void Stop()
        {
            if (!isRunning) return;

            try
            {
                isRunning = false;
                cts?.Cancel();
                listener?.Stop();
                Console.WriteLine("Server stopped.");
            }
            catch (Exception e)
            {
                OnError?.Invoke(null, e);
                Console.WriteLine($"Error stopping server: {e.Message}");
            }
            finally
            {
                cts?.Dispose();
                cts = null;
            }
        }
    }
}