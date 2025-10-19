using System;
using System.Linq;
using GrainInterfaces;
using kcp2k;
using Tutorial;

namespace Client
{
    // 进程上的观察者，用来监听逻辑服。
    public class PacketObserver : IPacketObserver
    {
        private readonly KcpServer context;

        public PacketObserver(KcpServer context)
        {
            this.context = context;
        }

        // 处理从逻辑服过来的消息，（那么客户端消息呢？）
        public void OnReceivePacket(ArraySegment<byte> Packet)
        {
            Console.WriteLine($"Silo.OnReceivePacket，返回给Client:{context}");

            var model = ProtobufferTool.Deserialize<TheMsg>(Packet.ToArray());
            Console.WriteLine($"解析:{model.Name}说: {model.Content}");

            TheMsg theMsg = new TheMsg { Name = "服务器", Content = "0x03, 0x04, Unreliable" };
            byte[] bytes = ProtobufferTool.Serialize(theMsg);
            ArraySegment<byte> data = new ArraySegment<byte>(bytes);

            int firstclient = context.connections.Count > 0 ? context.connections.First().Key : -1;
            context.Send(firstclient, data, KcpChannel.Unreliable);
        }
    }
}