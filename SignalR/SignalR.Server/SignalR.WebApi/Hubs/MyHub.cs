using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace Game.WebApi
{
    public class MyHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            Trace.WriteLine($"OnConnectedAsync - {Context.ConnectionId}");
            // 返回ConnectionId令牌
            await Clients.Client(Context.ConnectionId).SendAsync("OnConnected", Context.ConnectionId); //DAVzDwZcgOpeOTMAevZ0gw
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Trace.WriteLine($"OnDisconnectedAsync - {Context.ConnectionId}");
            return Task.CompletedTask;
        }

        #region RPC
        public async Task SendMessage(string user, string message)
        {
            Trace.WriteLine($"[C2S Receive] {user}:{message} → 广播");
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        #endregion
    }
}