using System;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class ServerScript : MonoBehaviour
{
    //public string serverIP = "localhost";
    public int serverPort = 3000;

    TcpListener server = null;
    TcpClient client = null;
    NetworkStream stream = null;
    Thread thread;

    void Awake()
    {
        GetLocalIPAddress();

        string _port = CommandLineGrabber.GetArg("--port");
        if (string.IsNullOrEmpty(_port))
        {
            Debug.LogError("none, none, none, ");
            return;
        }

        Debug.Log("CommandLineGrabberTest init");
        Debug.Log("grabbed port -> " + _port);
        serverPort = int.Parse(_port);
    }

    void Start()
    {
        thread = new Thread(new ThreadStart(SetupServer));
        thread.Start();
    }

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        SendMessageToClient("Hello");
    //    }
    //}

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
        server?.Stop();
        thread?.Abort();
    }

    void SetupServer()
    {
        try
        {
            //IPAddress localAddr = IPAddress.Parse(serverIP);
            //server = new TcpListener(localAddr, serverPort);
            server = new TcpListener(IPAddress.Any, serverPort);
            server.Start();

            byte[] buffer = new byte[1024];
            string data = null;

            while (true)
            {
                Debug.Log($"[S] Server is started on {serverPort}, Waiting for connection...");
                client = server.AcceptTcpClient();
                Debug.Log("[S] Connected!");

                data = null;
                stream = client?.GetStream();

                int i;

                while ((i = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    data = Encoding.UTF8.GetString(buffer, 0, i);
                    Debug.Log("[C2S] Received: " + data);

                    string response = "Server response: " + data.ToString();
                    SendMessageToClient(message: response);
                }
                client.Close();
            }
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e);
        }
        finally
        {
            server.Stop();
        }
    }

    void SendMessageToClient(string message)
    {
        byte[] msg = Encoding.UTF8.GetBytes(message);
        stream.Write(msg, 0, msg.Length);
        Debug.Log("[S2C] Send: " + message);
    }

    string GetLocalIPAddress()
    {
        //string hostName = Dns.GetHostName(); //获取本机主机名
        //IPHostEntry hostEntry = Dns.GetHostEntry(hostName); //获取与主机名关联的 IP 地址列表
        //IPAddress localIPAddress = hostEntry.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        //return localIPAddress.ToString();

        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                Console.WriteLine("IPs:" + ip.ToString());
            }
        }
        return host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToString();
    }
}