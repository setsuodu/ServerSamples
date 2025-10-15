using UnityEngine;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TCPClient : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Thread clientReceiveThread;

    // Server settings
    private string serverIP = "127.0.0.1"; // Replace with your server's IP
    private int port = 8080; // Replace with your server's port

    void Start()
    {
        ConnectToServer();
    }

    void ConnectToServer()
    {
        try
        {
            // Initialize TCP client
            client = new TcpClient();
            Debug.Log("Connecting to server...");
            client.Connect(serverIP, port);
            Debug.Log("Connected to server!");

            // Get the stream for reading and writing
            stream = client.GetStream();

            // Start a separate thread to handle receiving messages
            clientReceiveThread = new Thread(new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();

            // Send a test message to the server
            SendMessageToServer("Hello from Unity!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Connection error: " + e.Message);
        }
    }

    void SendMessageToServer(string message)
    {
        if (client == null || !client.Connected)
        {
            Debug.LogError("Not connected to server!");
            return;
        }

        try
        {
            // Convert the message to bytes
            byte[] data = Encoding.ASCII.GetBytes(message + "\n");
            // Send the message to the server
            stream.Write(data, 0, data.Length);
            Debug.Log("Sent: " + message);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error sending message: " + e.Message);
        }
    }

    void ListenForData()
    {
        try
        {
            byte[] bytes = new byte[1024];
            while (client.Connected)
            {
                // Read incoming data
                int bytesRead = stream.Read(bytes, 0, bytes.Length);
                if (bytesRead > 0)
                {
                    string message = Encoding.ASCII.GetString(bytes, 0, bytesRead);
                    Debug.Log("Received: " + message);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error receiving data: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        // Clean up when the application quits
        if (stream != null)
            stream.Close();
        if (client != null)
            client.Close();
        if (clientReceiveThread != null)
            clientReceiveThread.Abort();
    }

    [ContextMenu("Test_SendMessage")]
    public void Test_SendMessage()
    {
        SendMessageToServer("hello, I am unity");
    }
}