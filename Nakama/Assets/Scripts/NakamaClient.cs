using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Nakama;
using Nakama.TinyJson;

public class NakamaClient : MonoBehaviour
{
    private readonly IClient client;
    private ISocket socket;

    public string scheme = "http";
    public string host = "192.168.1.106";
    public int port = 7350; //7350是游戏服，7351是后台
    public string serverKey = "defaultkey";

    public string email = "hello@example.com";
    public string username = "hello";
    public string password = "12345678";

    public string RoomName = "heroes";

    public Button btn_CreateUserByEmail;
    public Button btn_CreateUserByDevice;
    public Button btn_AddFriend;
    public Button btn_AddScore;
    public Button btn_ConnectSocket;

    void Awake()
    {
        btn_CreateUserByEmail.onClick.AddListener(CreateUserByEmail);
        btn_CreateUserByDevice.onClick.AddListener(CreateUserByDevice);
        btn_AddFriend.onClick.AddListener(AddFriend);
        btn_AddScore.onClick.AddListener(AddScore);
        btn_ConnectSocket.onClick.AddListener(ConnectSocket);
    }

    void OnApplicationQuit()
    {
        socket?.CloseAsync(); //close safely
    }

    async void CreateUserByEmail()
    {
        var client = new Client(scheme, host, port, serverKey);
        try
        {
            var session = await client.AuthenticateEmailAsync(email, password, username, true);
            Debug.Log(session);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    async void CreateUserByDevice()
    {
        var client = new Client(scheme, host, port, serverKey);
        var deviceId = SystemInfo.deviceUniqueIdentifier;
        try
        {
            var session = await client.AuthenticateDeviceAsync(deviceId, username, true);
            Debug.Log(session);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    async void AddFriend()
    {
        var client = new Client(scheme, host, port, serverKey);
        try
        {
            var session = await client.AuthenticateEmailAsync(email, password);
            Debug.Log(session);

            List<string> ids = new List<string> { "101484ae-a8e2-470f-a053-7cbc074f60b1" };
            await client.AddFriendsAsync(session, ids);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    async void AddScore()
    {
        var client = new Client(scheme, host, port, serverKey);
        try
        {
            var session = await client.AuthenticateEmailAsync(email, password);
            Debug.Log(session);

            var score = new IApiWriteStorageObject[]
            {
                new Score { Collection = "match", Key = "pa", Value = "100"},
                new Score { Collection = "match", Key = "pb", Value = "200"},
            };
            var result = await client.WriteStorageObjectsAsync(session, score);
            Debug.Log(result);
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
        }
    }

    async void FindMatch()
    {

    }

    async void ConnectSocket()
    {
        // 登录Nakama
        var client = new Client(scheme, host, port, serverKey);
        var session = await client.AuthenticateEmailAsync(email, password);
        Debug.Log(session);

        // 创建Socket
        socket = client.NewSocket();
        socket.Connected += Socket_Connected; //Lambada function
        socket.Closed += Socket_Closed; ;
        socket.ReceivedError += Socket_ReceivedError;
        socket.ReceivedChannelMessage += Socket_ReceivedChannelMessage;
        socket.ReceivedMatchmakerMatched += Socket_ReceivedMatchmakerMatched;

        // 连接Socket
        await socket.ConnectAsync(session);
        // 加入一个Matchmaker
        await socket.AddMatchmakerAsync("*", 2, 2);

        // 模拟第二个玩家加入
        var deviceId2 = Guid.NewGuid().ToString();
        var session2 = await client.AuthenticateDeviceAsync(deviceId2);
        var socket2 = client.NewSocket();
        socket2.ReceivedMatchmakerMatched += async matched => await socket2.JoinMatchAsync(matched);
        await socket2.ConnectAsync(session2);
        await socket2.AddMatchmakerAsync("*", 2, 2);
        await Task.Delay(10000); //disconnect after 10sec
        Debug.Log("after delay, socket2 close");
        await socket2.CloseAsync();

        /*
        // 加入房间
        var channel = await socket.JoinChatAsync(RoomName, ChannelType.Room);
        Debug.Log($"join chat channel: {channel}");
        // 聊天格式 JsonObject
        var content = new Dictionary<string, string> { {"hello", "world" } }.ToJson();
        // 向channel发送
        _ = socket.WriteChatMessageAsync(channel, content);
        */
    }

    void Socket_Connected()
    {
        Debug.Log("socket connected.");
    }

    void Socket_Closed()
    {
        Debug.Log("socket closed.");
    }

    void Socket_ReceivedError(Exception obj)
    {
        Debug.LogError(obj.Message);
    }

    // 接收channel消息
    void Socket_ReceivedChannelMessage(IApiChannelMessage obj)
    {
        Debug.Log($"receive channel: {obj}");
    }

    // 接收匹配消息
    async void Socket_ReceivedMatchmakerMatched(IMatchmakerMatched obj)
    {
        Debug.Log($"receive matchmaker matched: {obj}");
        var match = await socket.JoinMatchAsync(obj);
        Debug.Log($"new match: {match}");

        var self = match.Self;
        Debug.Log($"self: {self}, Presences:{match.Presences}");
    }
}

public class Score : IApiWriteStorageObject
{
    public string Collection { get; set; }

    public string CreateTime { get; set; }

    public string Key { get; set; }

    public int PermissionRead  { get; set; }

    public int PermissionWrite  { get; set; }

    public string UpdateTime  { get; set; }

    public string UserId  { get; set; }

    public string Value { get; set; }

    public string Version  { get; set; }
}