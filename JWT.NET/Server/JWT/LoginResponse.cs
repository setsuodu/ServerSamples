public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class LoginResponse
{
    public string AccessToken { get; set; } // 设置短过期时间（比如 15 分钟）
    public string RefreshToken { get; set; } // 设置长过期时间（比如 7 天）
}
