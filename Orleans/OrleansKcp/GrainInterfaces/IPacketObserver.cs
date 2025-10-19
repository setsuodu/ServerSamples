using System;
using Orleans;

namespace GrainInterfaces
{
    // 观察者在网关
    public interface IPacketObserver : IGrainObserver
    {
        // 网关服收到逻辑服消息
        void OnReceivePacket(ArraySegment<byte> Packet);
    }
}
