using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ClientScript : MonoBehaviour
{
    public string serverIP = "localhost"; // Set this to your server's IP address.
    public int serverPort = 3000;         // Set this to your server's port.
    //private string messageToSend = "Hello Server!"; // The message to send.

    private TcpClient client;
    private NetworkStream stream;
    private Thread clientReceiveThread;

    public InputField m_IP;
    public InputField m_Port;
    public Button m_ConnectBtn;
    public InputField m_Message;
    public Button m_SendBtn;

    void Awake()
    {
        serverIP = m_IP.text;
        serverPort = int.Parse(m_Port.text);

        m_ConnectBtn.onClick.AddListener(ConnectToServer);
        m_SendBtn.onClick.AddListener(() =>
        {
            SendMessageToServer(m_Message.text);
        });
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
        clientReceiveThread?.Abort();
    }

    void ConnectToServer()
    {
        serverIP = m_IP.text;
        serverPort = int.Parse(m_Port.text);

        try
        {
            Debug.Log($"[C] Connected to server({serverIP}:{serverPort}).");
            client = new TcpClient(serverIP, serverPort);
            stream = client.GetStream();

            clientReceiveThread = new Thread(new ThreadStart(OnReceived));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException: " + e.ToString());
        }
    }

    void OnReceived()
    {
        try
        {
            byte[] bytes = new byte[1024];
            while (true)
            {
                // Check if there's any data available on the network stream
                if (stream.DataAvailable)
                {
                    int length;
                    // Read incoming stream into byte array.
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incomingData = new byte[length];
                        Array.Copy(bytes, 0, incomingData, 0, length);
                        // Convert byte array to string message.
                        string serverMessage = Encoding.UTF8.GetString(incomingData);
                        Debug.Log("[S2C] Server message received: " + serverMessage);
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    void SendMessageToServer(string message)
    {
        Debug.Log("[C2S] Send message to server: " + message);

        if (client == null || !client.Connected)
        {
            Debug.LogError("Client not connected to server.");
            return;
        }

        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);
    }
}