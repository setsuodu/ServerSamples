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
    // 信件
    protected class Envelope
    {
        public object Message { get; set; } // 消息内容
        public Actor Sender { get; set; } // 发件人
    }
}

// PlayerActor消息定义
public class PlayerMessages
{
    // 连接消息
    public class Connect
    {
        public TcpClient Client { get; set; }
    }
    // 断开连接消息
    public class Disconnect { }
    // 数据接收消息
    public class DataReceived
    {
        public int MessageId { get; set; }
        public IMessage Message { get; set; }
    }
    // 发送到客户端消息
    public class SendToClient
    {
        public int MessageId { get; set; }
        public IMessage Message { get; set; }
    }
    // 获取玩家信息消息
    public class GetPlayerInfo
    {
        public UserInfo UserInfo { get; set; }
    }
}

// PlayerManager消息定义
public class ManagerMessages
{
    // 创建玩家消息
    public class CreatePlayer
    {
        public TcpClient Client { get; set; }
        public TaskCompletionSource<PlayerActor> Completion { get; set; }
    }
    // 移除玩家消息
    public class RemovePlayer
    {
        public PlayerActor Player { get; set; }
    }
    // 通过TcpClient获取玩家消息
    public class GetPlayerByClient
    {
        public TcpClient Client { get; set; }
        public TaskCompletionSource<PlayerActor> Completion { get; set; }
    }
    // 广播消息
    public class BroadcastMessage
    {
        public int MessageId { get; set; }
        public IMessage Message { get; set; }
    }
    // 获取所有玩家消息
    public class GetAllPlayers
    {
        public TaskCompletionSource<PlayerActor[]> Completion { get; set; }
    }
}