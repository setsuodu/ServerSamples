// 消息结构
public class Message
{
    public int MessageId { get; set; }
    public byte[] Data { get; set; }
}

public enum MessageType
{
    Login = 1,
    Logout = 2,
    GetFriendList = 3
}