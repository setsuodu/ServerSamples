using System;
using System.Text;
using System.Net.Sockets;
using UnityEngine;

namespace ClientSync
{
    public class Client : MonoBehaviour
    {
        private TcpClient client;
        private NetworkStream stream;
        private bool isConnected;

        void Start()
        {
            ConnectToServer(GameConfigs.SERVER_IP, GameConfigs.SERVER_PORT);
        }

        void OnDestroy()
        {
            Disconnect();
        }

        void Update()
        {
            if (!isConnected) return;

            try
            {
                // 检查是否有数据可读
                if (stream.DataAvailable)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Debug.Log($"[S] Received from server: {message}");
                }

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
            catch (Exception e)
            {
                Debug.LogError($"Error in Update: {e.Message}");
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
                Debug.Log($"[C] Connected to server: {ipAddress}:{port}"); // 连接成功

                // 发送一条初始消息
                //SendMessageToServer("Hello from Unity Client!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Connection error: {e.Message}");
                isConnected = false;
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
            isConnected = false;
            stream?.Close();
            client?.Close();
            Debug.Log("[C] Disconnected from server.");
        }
    }
}