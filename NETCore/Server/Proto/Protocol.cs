// 消息结构
public class Message
{
    public int MessageId { get; set; }
    public byte[] Data { get; set; }
}
// 定义消息号枚举
public enum MsgId
{
    NONE = 0,
    // 客户端 => 服务端
    C2S_LOGIN = 1001, //登录请求
    C2S_LOGOUT = 1002, //登出请求
    C2S_FRIENDLIST = 1003, //好友列表请求
    // 服务端 => 客户端
    S2C_LOGIN = 1004, //登录结果
    S2C_LOGOUT = 1005, //登出结果
    S2C_FRIENDLIST = 1006, //好友列表结果
}
/// <summary>
/// 消息号枚举，定义客户端与服务器之间的消息类型
/// </summary>
public enum MessageId : ushort
{
    // 系统消息 (0x0000 - 0x0FFF)
    Heartbeat = 0x0001,         // 心跳包
    LoginRequest = 0x0002,      // 登录请求
    LoginResponse = 0x0003,     // 登录响应
    Disconnect = 0x0004,        // 断开连接通知

    // 游戏逻辑消息 (0x1000 - 0x1FFF)
    MoveRequest = 0x1001,       // 玩家移动请求
    MoveResponse = 0x1002,      // 玩家移动响应
    AttackRequest = 0x1003,     // 攻击请求
    AttackResponse = 0x1004,    // 攻击响应
    ChatMessage = 0x1005,       // 聊天消息

    // 房间相关消息 (0x2000 - 0x2FFF)
    JoinRoomRequest = 0x2001,   // 加入房间请求
    JoinRoomResponse = 0x2002,  // 加入房间响应
    LeaveRoomRequest = 0x2003,  // 离开房间请求
    LeaveRoomResponse = 0x2004, // 离开房间响应

    // 其他消息 (0x3000 - 0xFFFF)
    ErrorMessage = 0x3001,      // 错误消息
    ServerBroadcast = 0x3002    // 服务器广播消息
}