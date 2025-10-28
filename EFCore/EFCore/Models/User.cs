// Models/User.cs
public class User
{
    public long Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;

    public bool IsActive { get; set; } = true;
    public bool IsBanned { get; set; } = false;
    public DateTime? BannedUntil { get; set; }
    public string? BannedReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    // 导航属性
    public ICollection<Character> Characters { get; set; } = new List<Character>();
    public ICollection<LoginLog> LoginLogs { get; set; } = new List<LoginLog>();
    public ICollection<ThirdPartyLogin> ThirdPartyLogins { get; set; } = new List<ThirdPartyLogin>();
    public ICollection<BanRecord> BanRecords { get; set; } = new List<BanRecord>();

    // 我发起的好友关系
    public ICollection<Friend> Friends { get; set; } = new List<Friend>();

    // 别人加我的（我被加为好友）
    public ICollection<Friend> FriendOf { get; set; } = new List<Friend>();
}
