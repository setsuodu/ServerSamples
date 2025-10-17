using System.Net.Sockets;
using System.Collections.Concurrent;
using Google.Protobuf;

public class UserInfo
{
    public string UserId { get; set; }
    public string Username { get; set; }
}

public class PlayerManager : Actor
{
    private readonly ConcurrentDictionary<string, PlayerActor> _players = new ConcurrentDictionary<string, PlayerActor>();
    private readonly ConcurrentDictionary<TcpClient, PlayerActor> _clientToPlayerMap = new ConcurrentDictionary<TcpClient, PlayerActor>();

    protected override async Task ReceiveAsync(object message, Actor sender)
    {
        switch (message)
        {
            case ManagerMessages.CreatePlayer createMsg:
                await HandleCreatePlayerAsync(createMsg);
                break;
            case ManagerMessages.RemovePlayer removeMsg:
                await HandleRemovePlayerAsync(removeMsg.Player);
                break;
            case ManagerMessages.GetPlayerByClient getPlayerMsg:
                HandleGetPlayerByClient(getPlayerMsg);
                break;
            case ManagerMessages.BroadcastMessage broadcastMsg:
                await HandleBroadcastMessageAsync(broadcastMsg);
                break;
            case ManagerMessages.GetAllPlayers getAllMsg:
                HandleGetAllPlayers(getAllMsg);
                break;
            default:
                Console.WriteLine($"PlayerManager received unknown message: {message.GetType().Name}");
                break;
        }
    }

    private async Task HandleCreatePlayerAsync(ManagerMessages.CreatePlayer createMsg)
    {
        var player = new PlayerActor(this);
        await player.StartAsync();

        // 发送连接消息给PlayerActor
        player.SendMessage(player, new PlayerMessages.Connect { Client = createMsg.Client });

        // 存储玩家
        string playerKey = createMsg.Client.Client.RemoteEndPoint.ToString();
        _players[playerKey] = player;
        _clientToPlayerMap[createMsg.Client] = player;

        Console.WriteLine($"PlayerManager created PlayerActor for: {createMsg.Client.Client.RemoteEndPoint}");
        createMsg.Completion.SetResult(player);
    }

    private async Task HandleRemovePlayerAsync(PlayerActor player)
    {
        string playerKey = _players.FirstOrDefault(p => p.Value == player).Key;
        if (playerKey != null)
        {
            _players.TryRemove(playerKey, out _);
        }

        var clientKey = _clientToPlayerMap.FirstOrDefault(p => p.Value == player).Key;
        if (clientKey != null)
        {
            _clientToPlayerMap.TryRemove(clientKey, out _);
        }

        await player.StopAsync();
        Console.WriteLine($"PlayerManager removed PlayerActor: {player.PlayerId}");
    }

    private void HandleGetPlayerByClient(ManagerMessages.GetPlayerByClient getPlayerMsg)
    {
        if (_clientToPlayerMap.TryGetValue(getPlayerMsg.Client, out PlayerActor player))
        {
            getPlayerMsg.Completion.SetResult(player);
        }
        else
        {
            getPlayerMsg.Completion.SetResult(null);
        }
    }

    private async Task HandleBroadcastMessageAsync(ManagerMessages.BroadcastMessage broadcastMsg)
    {
        var tasks = _players.Values.Select(player =>
        {
            player.SendMessage(player, new PlayerMessages.SendToClient
            {
                MessageId = broadcastMsg.MessageId,
                Message = broadcastMsg.Message
            });
            return Task.CompletedTask;
        });
        await Task.WhenAll(tasks);
        Console.WriteLine($"PlayerManager broadcasted message to {_players.Count} players");
    }

    private void HandleGetAllPlayers(ManagerMessages.GetAllPlayers getAllMsg)
    {
        var players = _players.Values.ToArray();
        getAllMsg.Completion.SetResult(players);
    }

    public async Task<PlayerActor> CreatePlayerAsync(TcpClient client)
    {
        var tcs = new TaskCompletionSource<PlayerActor>();
        SendMessage(this, new ManagerMessages.CreatePlayer { Client = client, Completion = tcs });
        return await tcs.Task;
    }

    public async Task RemovePlayerAsync(PlayerActor player)
    {
        SendMessage(this, new ManagerMessages.RemovePlayer { Player = player });
        await Task.Delay(100); // 等待处理完成
    }

    public async Task<PlayerActor> GetPlayerByClientAsync(TcpClient client)
    {
        var tcs = new TaskCompletionSource<PlayerActor>();
        SendMessage(this, new ManagerMessages.GetPlayerByClient { Client = client, Completion = tcs });
        return await tcs.Task;
    }

    public async Task BroadcastMessageAsync(int messageId, IMessage message)
    {
        SendMessage(this, new ManagerMessages.BroadcastMessage
        {
            MessageId = messageId,
            Message = message
        });
        await Task.Delay(100); // 等待广播完成
    }

    public async Task<PlayerActor[]> GetAllPlayersAsync()
    {
        var tcs = new TaskCompletionSource<PlayerActor[]>();
        SendMessage(this, new ManagerMessages.GetAllPlayers { Completion = tcs });
        return await tcs.Task;
    }

    public override async Task StopAsync()
    {
        var players = _players.Values.ToArray();
        foreach (var player in players)
        {
            await RemovePlayerAsync(player);
        }
        _players.Clear();
        _clientToPlayerMap.Clear();
        await base.StopAsync();
    }
}