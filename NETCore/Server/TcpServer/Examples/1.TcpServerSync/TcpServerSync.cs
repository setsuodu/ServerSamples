using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpServerSync
{
    public class TcpServer
    {
        private TcpListener listener;
        private bool isRunning;

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
                    Console.WriteLine($"[C] Client connected: {client.Client.RemoteEndPoint}"); // 监听到客户端连接成功

                    // 为每个客户端创建一个线程处理
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.Start();
                }
            }
            catch (Exception e)
            {
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
                int bytesRead;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"[C] Received from client {client.Client.RemoteEndPoint}: {message}");

                    // 回送消息给客户端
                    string response = $"Server received: {message}";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                    Console.WriteLine($"[S] Sent to client: {response}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Client error: {e.Message}");
            }
            finally
            {
                client.Close();
                Console.WriteLine($"Client disconnected: {client.Client?.RemoteEndPoint}");
            }
        }

        public void Stop()
        {
            isRunning = false;
            listener?.Stop();
            Console.WriteLine("Server stopped.");
        }
    }
}