// Models/LoginLog.cs
public class LoginLog
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
    public DateTime LoginAt { get; set; }

    public User User { get; set; } = null!;
}
