using System;
using System.Threading.Tasks;
using GrainInterfaces;
using Orleans.Runtime;

namespace Grains
{
    public class PacketRouterGrain : Orleans.Grain, IPacketRouterGrain
    {
        private IPacketObserver observer;

        // 通过绑定观察者，逻辑服向网关发数据
        //public Task OnReceivePacket(NetPacket packet)
        public Task OnReceivePacket(ArraySegment<byte> packet)
        {
            // 当前Grain的Key
            long id = GrainReference.GrainIdentity.PrimaryKeyLong;
            Console.WriteLine($"逻辑服 {id} 收到NetPacket");

            //if (packet.head == 0)
            {
                // 发回给网关消息
                observer.OnReceivePacket(packet);
            }

            return Task.CompletedTask;
        }

        // 网关服务器Rpc调用，绑定观察者。
        // 因为这个类是属于逻辑服的，网关服务器通过接口可以找到这个类
        public Task BindPacketObserver(IPacketObserver observer)
        {
            this.observer = observer;

            Console.WriteLine($"绑定：");

            return Task.CompletedTask;
        }
    }
}
