using System;
using UnityEngine;
using Tutorial;

namespace kcp2k.Examples
{
    public class TestClient : MonoBehaviour
    {
        // configuration
        public ushort Port = 7777;

        // client
        public KcpClient client;

        // MonoBehaviour ///////////////////////////////////////////////////////
        void Awake()
        {
            // logging
            Log.Info = Debug.Log;
            Log.Warning = Debug.LogWarning;
            Log.Error = Debug.LogError;

            client = new KcpClient(
                OnConnected,
               (message, channel) => OnData(message, channel),
                OnDisconnected,
                (error, reason) => OnError(error, reason)
            );
        }

        public void LateUpdate() => client.Tick();

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(5, 5, 150, 400));
            GUILayout.Label("Client:");
            if (GUILayout.Button("Connect 127.0.0.1"))
            {
                client.Connect("127.0.0.1", Port, true, 10);
            }
            if (GUILayout.Button("Send 0x01, 0x02 reliable"))
            {
                TheMsg msg = new TheMsg { Name = "tom", Content = "chat content: 0x01, 0x02 reliable" };
                ArraySegment<byte> data = ProtobufferTool.Serialize(msg);
                //ArraySegment<byte> data = new ArraySegment<byte>(new byte[] { 0x01, 0x02 });
                client.Send(data, KcpChannel.Reliable);
            }
            if (GUILayout.Button("Send 0x03, 0x04 unreliable"))
            {
                TheMsg msg = new TheMsg { Name = "tom", Content = "chat content: 0x03, 0x04 unreliable" };
                ArraySegment<byte> data = ProtobufferTool.Serialize(msg);
                //ArraySegment<byte> data = new ArraySegment<byte>(new byte[] { 0x03, 0x04 });
                client.Send(data, KcpChannel.Unreliable);
            }
            if (GUILayout.Button("Disconnect"))
            {
                client.Disconnect();
            }
            GUILayout.EndArea();
        }

        //Action OnConnected,
        void OnConnected()
        {
            Debug.Log("OnConnected");
        }
        //Action<ArraySegment<byte>, KcpChannel> OnData,
        void OnData(ArraySegment<byte> message, KcpChannel channel)
        {
            Debug.Log($"KCP: OnClientDataReceived({BitConverter.ToString(message.Array, message.Offset, message.Count)} @ {channel})");

            var model = ProtobufferTool.Deserialize<TheMsg>(message.ToArray());
            Debug.Log($"解析: {model.Name}说: {model.Content}");
        }
        //
        //Action OnDisconnected,
        void OnDisconnected()
        {
            Debug.Log("OnDisconnected");
        }
        //Action<ErrorCode, string> OnError)
        void OnError(ErrorCode error, string reason)
        {
            Debug.LogWarning($"KCP: OnClientError({error}, {reason}");
        }
    }
}