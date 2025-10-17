using System.Net.Sockets;
using Google.Protobuf;

public class PlayerActor : Actor
{
    private TcpClient _client;
    private NetworkStream _stream;
    private UserInfo _userInfo;
    private bool _isConnected;
    private readonly PlayerManager _playerManager;
    private readonly CancellationTokenSource _cts;

    public string PlayerId => _userInfo?.UserId ?? "Unknown";
    public string Username => _userInfo?.Username ?? "Unknown";
    public TcpClient Client => _client;

    public PlayerActor(PlayerManager playerManager)
    {
        _playerManager = playerManager;
        _cts = new CancellationTokenSource();
    }

    protected override async Task ReceiveAsync(object message, Actor sender)
    {
        switch (message)
        {
            case PlayerMessages.Connect connectMsg:
                await HandleConnectAsync(connectMsg.Client);
                break;
            case PlayerMessages.DataReceived dataMsg:
                await HandleDataReceivedAsync(dataMsg.MessageId, dataMsg.Message);
                break;
            case PlayerMessages.SendToClient sendMsg:
                await HandleSendToClientAsync(sendMsg.MessageId, sendMsg.Message);
                break;
            case PlayerMessages.Disconnect _:
                await HandleDisconnectAsync();
                break;
            case PlayerMessages.GetPlayerInfo getInfoMsg:
                getInfoMsg.UserInfo = _userInfo;
                break;
            default:
                Console.WriteLine($"PlayerActor {PlayerId} received unknown message: {message.GetType().Name}");
                break;
        }
    }

    private async Task HandleConnectAsync(TcpClient client)
    {
        _client = client;
        _stream = client.GetStream();
        _isConnected = true;

        Console.WriteLine($"PlayerActor connected: {client.Client.RemoteEndPoint}");

        // 启动数据接收任务
        _ = Task.Run(() => ReceiveDataLoopAsync(), _cts.Token);
    }

    private async Task ReceiveDataLoopAsync()
    {
        try
        {
            while (_isConnected && !_cts.Token.IsCancellationRequested)
            {
                (int messageId, IMessage message) = await ReadMessageAsync(_stream);
                if (message == null) break;

                // 通过Actor消息系统发送数据
                SendMessage(this, new PlayerMessages.DataReceived
                {
                    MessageId = messageId,
                    Message = message
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PlayerActor {PlayerId} receive error: {ex.Message}");
            await HandleDisconnectAsync();
        }
    }

    private async Task<(int, IMessage)> ReadMessageAsync(NetworkStream stream)
    {
        try
        {
            // 读取消息长度（4字节）
            byte[] lengthBuffer = new byte[4];
            int bytesRead = await stream.ReadAsync(lengthBuffer, 0, 4, _cts.Token);
            if (bytesRead != 4) return (0, null);

            int length = BitConverter.ToInt32(lengthBuffer, 0);

            // 读取消息号（4字节）+ 消息体
            byte[] messageBuffer = new byte[length];
            bytesRead = await stream.ReadAsync(messageBuffer, 0, length, _cts.Token);
            if (bytesRead != length) return (0, null);

            int messageId = BitConverter.ToInt32(messageBuffer, 0);
            byte[] protoData = new byte[length - 4];
            Array.Copy(messageBuffer, 4, protoData, 0, length - 4);

            // 根据消息号解析 Protobuf 消息
            IMessage message = (MsgId)messageId switch
            {
                MsgId.C2S_LOGIN => LoginRequest.Parser.ParseFrom(protoData),
                MsgId.C2S_LOGOUT => EmptyRequest.Parser.ParseFrom(protoData),
                _ => throw new Exception($"Unknown message ID: {messageId}")
            };

            return (messageId, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ReadMessage error: {ex.Message}");
            return (0, null);
        }
    }

    private async Task HandleDataReceivedAsync(int messageId, IMessage message)
    {
        Console.WriteLine($"PlayerActor {PlayerId} received message ID: {messageId}");

        switch ((MsgId)messageId)
        {
            case MsgId.S2C_LOGIN:
                await HandleLoginAsync(message as LoginRequest);
                break;
            case MsgId.C2S_LOGOUT:
                await HandleLogoutAsync();
                break;
            default:
                Console.WriteLine($"PlayerActor {PlayerId} unknown message ID: {messageId}");
                break;
        }
    }

    private async Task HandleLoginAsync(LoginRequest request)
    {
        Console.WriteLine($"PlayerActor handling login: {request.Username}");

        // 模拟登录逻辑
        bool success = request.Username == "admin" && request.Password == "password";
        var response = new LoginResponse { Code = 0 };

        // 如果登录成功，记录用户信息
        if (success)
        {
            _userInfo = new UserInfo
            {
                UserId = Guid.NewGuid().ToString(),
                Username = request.Username
            };
            Console.WriteLine($"PlayerActor {PlayerId} logged in as: {_userInfo.Username}");
        }

        // 发送响应
        await SendMessageAsync((int)MsgId.S2C_LOGIN, response);
    }

    private async Task HandleLogoutAsync()
    {
        Console.WriteLine($"PlayerActor {_userInfo.Username}");

        var response = new LogoutResponse { Code = 0 };

        await SendMessageAsync((int)MsgId.S2C_LOGOUT, response);
    }

    private async Task HandleSendToClientAsync(int messageId, IMessage message)
    {
        if (!_isConnected) return;

        try
        {
            using var ms = new MemoryStream();
            message.WriteTo(ms);
            byte[] protoData = ms.ToArray();
            byte[] messageIdBytes = BitConverter.GetBytes(messageId);
            byte[] lengthBytes = BitConverter.GetBytes(protoData.Length + 4);

            byte[] fullMessage = new byte[lengthBytes.Length + messageIdBytes.Length + protoData.Length];
            Array.Copy(lengthBytes, 0, fullMessage, 0, lengthBytes.Length);
            Array.Copy(messageIdBytes, 0, fullMessage, lengthBytes.Length, messageIdBytes.Length);
            Array.Copy(protoData, 0, fullMessage, lengthBytes.Length + messageIdBytes.Length, protoData.Length);

            await _stream.WriteAsync(fullMessage, 0, fullMessage.Length, _cts.Token);
            Console.WriteLine($"PlayerActor {PlayerId} sent message ID {messageId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PlayerActor {PlayerId} send error: {ex.Message}");
            await HandleDisconnectAsync();
        }
    }

    private async Task SendMessageAsync(int messageId, IMessage message)
    {
        SendMessage(this, new PlayerMessages.SendToClient
        {
            MessageId = messageId,
            Message = message
        });
    }

    private async Task HandleDisconnectAsync()
    {
        if (!_isConnected) return;

        _isConnected = false;
        _cts.Cancel();

        try
        {
            _stream?.Close();
            _client?.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"PlayerActor {PlayerId} disconnect error: {ex.Message}");
        }

        // 通知PlayerManager移除这个玩家
        _playerManager.SendMessage(_playerManager, new ManagerMessages.RemovePlayer { Player = this });
        Console.WriteLine($"PlayerActor {PlayerId} disconnected");
    }

    public override async Task StopAsync()
    {
        await HandleDisconnectAsync();
        await base.StopAsync();
    }
}