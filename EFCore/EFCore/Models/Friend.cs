// Models/Friend.cs
public class Friend
{
    public long Id { get; set; }

    public long UserId { get; set; }
    public long FriendUserId { get; set; }

    public FriendStatus Status { get; set; } = FriendStatus.Pending;

    public string? ApplyMessage { get; set; }
    public string? RemarkName { get; set; }
    public string FriendGroup { get; set; } = "默认分组";

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // 导航属性
    public User User { get; set; } = null!;
    public User FriendUser { get; set; } = null!;
}

// 枚举：好友状态
public enum FriendStatus : short
{
    Pending = 0,    // 待确认
    Accepted = 1,   // 已好友
    Rejected = 2,   // 已拒绝
    Deleted = 3,    // 已删除（仍保留记录）
    Blocked = 4     // 黑名单
}