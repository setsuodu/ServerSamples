// RefreshTokenInfo.cs
public class RefreshTokenInfo
{
    public string Token { get; init; }
    public string Username { get; init; }
    public DateTime ExpiryDate { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public bool IsRevoked { get; private set; } = false;

    public void Revoke() => IsRevoked = true;
}