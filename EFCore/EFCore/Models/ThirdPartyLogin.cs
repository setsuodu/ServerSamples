// Models/ThirdPartyLogin.cs
public class ThirdPartyLogin
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Provider { get; set; } = null!; // google, apple, wechat...
    public string ProviderId { get; set; } = null!;
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}