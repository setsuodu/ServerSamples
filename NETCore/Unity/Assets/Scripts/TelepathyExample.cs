using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TelepathyExample : MonoBehaviour
{
    Telepathy.Client client = new Telepathy.Client(1024);
    Telepathy.Server server = new Telepathy.Server(1024);

    void Awake()
    {
        Debug.Log("Awake");

        Application.runInBackground = true;
        Telepathy.Log.Info = (string msg) => Debug.Log(msg);
        Telepathy.Log.Warning = (string msg) => Debug.LogWarning(msg);
        Telepathy.Log.Error = (string msg) => Debug.LogError(msg);

        client.OnConnected = () => Debug.Log("Client: OnConnected");
        client.OnDisconnected = () => Debug.Log("Client: OnDisconnected");
        client.OnData = (message) => Debug.Log("Client: OnData " + message);

        server.OnConnected = (connectionId, message) => Debug.Log("Server: OnConnected " + connectionId + " | " + message);
        server.OnDisconnected = (connectionId) => Debug.Log("Server: OnDisconnected " + connectionId);
        server.OnData = (connectionId, message) => Debug.Log("Server: OnData " + connectionId + " | " + message);

        server.Start(8080);
        client.Connect("localhost", 8080);
    }

    void Update()
    {
        client.Tick(100);
        if (client.Connected && Input.GetKeyDown(KeyCode.Space))
        {
            client.Send(new System.ArraySegment<byte>(new byte[] { 0x1 }));
            Debug.Log($"C2S 1");
        }

        server.Tick(100);
        if (server.Active && Input.GetKeyDown(KeyCode.P))
        {
            var clientId = 1;
            server.Send(clientId, new System.ArraySegment<byte>(new byte[] { 0x2 }));
            Debug.Log($"S2C 2");
        }
    }

    void OnApplicationQuit()
    {
        client.Disconnect();
        server.Stop();
    }
}
