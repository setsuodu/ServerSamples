using System.Net.Sockets;
using System.Collections.Concurrent;
using Google.Protobuf;

// Actor基类
public abstract class Actor
{
    private readonly ConcurrentQueue<Envelope> _mailbox = new ConcurrentQueue<Envelope>();
    private bool _isRunning;
    private Task _actorTask;
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    protected Actor()
    {
        _isRunning = true;
    }

    public async Task StartAsync()
    {
        _actorTask = ProcessMessagesAsync();
    }

    public virtual async Task StopAsync()
    {
        _isRunning = false;
        _cancellationTokenSource.Cancel();
        if (_actorTask != null)
        {
            await _actorTask;
        }
        _cancellationTokenSource.Dispose();
    }

    private async Task ProcessMessagesAsync()
    {
        while (_isRunning)
        {
            if (_mailbox.TryDequeue(out Envelope envelope))
            {
                try
                {
                    await ReceiveAsync(envelope.Message, envelope.Sender);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Actor {GetType().Name} error: {ex.Message}");
                }
            }
            else
            {
                await Task.Delay(10, _cancellationTokenSource.Token); // 避免忙等待
            }
        }
    }

    protected abstract Task ReceiveAsync(object message, Actor sender);

    public void SendMessage<T>(Actor target, T message)
    {
        target._mailbox.Enqueue(new Envelope { Message = message, Sender = this });
    }

    protected class Envelope
    {
        public object Message { get; set; }
        public Actor Sender { get; set; }
    }
}

// 玩家Actor消息定义
public class PlayerMessages
{
    public class Connect { public TcpClient Client { get; set; } }
    public class Disconnect { }
    public class DataReceived { public int MessageId { get; set; } public IMessage Message { get; set; } }
    public class SendToClient { public int MessageId { get; set; } public IMessage Message { get; set; } }
    public class GetPlayerInfo { public UserInfo UserInfo { get; set; } }
}

// PlayerManager消息定义
public class ManagerMessages
{
    public class CreatePlayer { public TcpClient Client { get; set; } public TaskCompletionSource<PlayerActor> Completion { get; set; } }
    public class RemovePlayer { public PlayerActor Player { get; set; } }
    public class GetPlayerByClient { public TcpClient Client { get; set; } public TaskCompletionSource<PlayerActor> Completion { get; set; } }
    public class BroadcastMessage { public int MessageId { get; set; } public IMessage Message { get; set; } }
    public class GetAllPlayers { public TaskCompletionSource<PlayerActor[]> Completion { get; set; } }
}