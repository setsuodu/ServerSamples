namespace EFCore.DTOs.Auth;

public class LoginResponse
{
    public string Token { get; set; } = null!;
    public long UserId { get; set; }
    public string Username { get; set; } = null!;
}