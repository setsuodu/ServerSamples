using System;
using System.Threading.Tasks;
using Orleans;

namespace GrainInterfaces
{
    // 这个Grain在逻辑服
    public interface IPacketRouterGrain : IGrainWithIntegerKey
    {
        // 逻辑服接收网关服消息
        //Task OnReceivePacket(NetPacket packet);
        Task OnReceivePacket(ArraySegment<byte> packet);

        // 通过绑定观察者，逻辑服向网关发数据
        Task BindPacketObserver(IPacketObserver observer);
    }
}