using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpServerSync
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

        // 事件定义
        public event ClientConnectedHandler OnConnect;
        public event ClientDisconnectedHandler OnDisconnect;
        public event DataReceivedHandler OnData;
        public event ErrorHandler OnError;

        public TcpServer(string ipAddress, int port)
        {
            listener = new TcpListener(IPAddress.Parse(ipAddress), port);
        }

        public void Start()
        {
            try
            {
                listener.Start();
                isRunning = true;
                Console.WriteLine($"Server started on {listener.LocalEndpoint}"); // 服务器启动成功

                while (isRunning)
                {
                    Console.WriteLine("Waiting for a connection...");
                    // 阻塞等待客户端连接
                    TcpClient client = listener.AcceptTcpClient();
                    OnConnect?.Invoke(client);
                    Console.WriteLine($"[C] Client connected: {client.Client.RemoteEndPoint}"); // 监听到客户端连接成功

                    // 为每个客户端创建一个线程处理
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.Start();
                }
            }
            catch (Exception e)
            {
                OnError?.Invoke(null, e);
                Console.WriteLine($"Server error: {e.Message}");
            }
            finally
            {
                Stop();
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];

                while (client.Connected)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        Console.WriteLine($"客户端主动断开");
                        //OnDisconnect?.Invoke(client); //客户端发送Disconnect，在这里收到。这里不处理，放到finally去。
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    OnData?.Invoke(client, message);
                    Console.WriteLine($"[C] Received from client {client.Client.RemoteEndPoint}: {message}");

                    // 回送消息给客户端
                    string response = $"Server received: {message}";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                    Console.WriteLine($"[S] Sent to client: {response}");
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
            catch (Exception e)
            {
                OnError?.Invoke(client, e);
                Console.WriteLine($"Unexpected error for client {client.Client.RemoteEndPoint}: {e.Message}");
            }
            finally
            {
                OnDisconnect?.Invoke(client);
                client.Close();
                Console.WriteLine($"Client disconnected: {client.Client?.RemoteEndPoint}");
            }
        }

        public void Stop()
        {
            try
            {
                isRunning = false;
                listener?.Stop();
                Console.WriteLine("Server stopped.");
            }
            catch (Exception e)
            {
                OnError?.Invoke(null, e);
                Console.WriteLine($"Error stopping server: {e.Message}");
            }
        }
    }
}