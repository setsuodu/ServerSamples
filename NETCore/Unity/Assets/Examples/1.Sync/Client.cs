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

        void ConnectToServer(string ipAddress, int port)
        {
            try
            {
                client = new TcpClient();
                client.Connect(ipAddress, port);
                stream = client.GetStream();
                isConnected = true;
                Debug.Log($"Connected to server: {ipAddress}:{port}");

                // 发送一条初始消息
                SendMessageToServer("Hello from Unity Client!");
            }
            catch (Exception e)
            {
                Debug.LogError($"Connection error: {e.Message}");
                isConnected = false;
            }
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
                    Debug.Log($"Received from server: {message}");
                }

                // 示例：按下空格键发送消息
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    SendMessageToServer("Unity client pressed Space!");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in Update: {e.Message}");
                Disconnect();
            }
        }

        void SendMessageToServer(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                Debug.Log($"Sent to server: {message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Send error: {e.Message}");
                Disconnect();
            }
        }

        void Disconnect()
        {
            isConnected = false;
            stream?.Close();
            client?.Close();
            Debug.Log("Disconnected from server.");
        }

        void OnDestroy()
        {
            Disconnect();
        }
    }
}