using Microsoft.AspNetCore.SignalR;

namespace SignalRServer.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            Console.WriteLine($"②【{DateTime.Now.ToString("HH:mm:ss.fff")}】[RPC] Client Send user={user}, message={message}");
            await Clients.All.SendAsync("ReceiveMessage", user, message);
            Console.WriteLine($"③【{DateTime.Now.ToString("HH:mm:ss.fff")}】tell all clients by ReceiveMessage");
        }
    }
}