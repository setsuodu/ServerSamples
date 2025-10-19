using System;
using System.Linq;
using kcp2k;
using Orleans;
using GrainInterfaces;
using System.Threading.Tasks;

namespace Client
{
    public class GameServer
    {
        private readonly IClusterClient client;

        private static IPacketRouterGrain routerGrain;

        private static PacketObserver packetObserver;

        // configuration
        public ushort Port = 7777;
        // server
        public KcpServer server;

        public GameServer(IClusterClient _client)
        {
            this.client = _client;

            server = new KcpServer(
                (connectionId) => OnConnected(connectionId),
                (connectionId, message, channel) => OnData(connectionId, message, channel),
                (connectionId) => OnDisconnected(connectionId),
                (connectionId, error, reason) => OnError(connectionId, error, reason),
                false,
                true,
                10
            );
        }

        //Action<int> OnConnected,
        void OnConnected(int connectionId)
        {
            Console.WriteLine($"KCP: {connectionId} is connected");


            routerGrain = client.GetGrain<IPacketRouterGrain>(123);

            //int firstclient = server.connections.Count > 0 ? server.connections.First().Key : -1;
            //server.Send(firstclient, new ArraySegment<byte>(new byte[] { 0x03, 0x04 }), KcpChannel.Unreliable);

            packetObserver = new PacketObserver(server);

            IPacketObserver observerRef = client.CreateObjectReference<IPacketObserver>(packetObserver).Result;

            routerGrain.BindPacketObserver(observerRef).Wait();

            Console.WriteLine($"{connectionId} 连接成功");
        }
        //Action<int, ArraySegment<byte>, KcpChannel> OnData,
        void OnData(int connectionId, ArraySegment<byte> message, KcpChannel channel)
        {
            Console.WriteLine($"KCP: OnServerDataReceived({connectionId}, {BitConverter.ToString(message.Array, message.Offset, message.Count)} @ {channel})");


            routerGrain.OnReceivePacket(message);
        }
        //Action<int> OnDisconnected,
        void OnDisconnected(int connectionId)
        {
            Console.WriteLine($"KCP: {connectionId} disconnected");
        }
        //Action<int, ErrorCode, string> OnError,
        void OnError(int connectionId, kcp2k.ErrorCode error, string reason)
        {
            Console.WriteLine($"KCP: OnServerError({connectionId}, {error}, {reason}");
        }


        public void LateUpdate() => server.Tick();

        public async Task Start()
        {
            server.Start(Port);

            while (true)
            {
                LateUpdate();
                //Thread.Sleep(15);
                await Task.Delay(15);
            }
        }

        public void Send()
        {
            int firstclient = server.connections.Count > 0 ? server.connections.First().Key : -1;

            server.Send(firstclient, new ArraySegment<byte>(new byte[] { 0x03, 0x04 }), KcpChannel.Unreliable);
        }

        public void Disconnect()
        {
            int firstclient = server.connections.Count > 0 ? server.connections.First().Key : -1;

            server.Disconnect(firstclient);
        }

        public void Stop()
        {
            server.Stop();
        }
    }
}