using System.Collections.Concurrent;

public static class TokenStorage
{
    // token -> RefreshTokenInfo
    private static readonly ConcurrentDictionary<string, RefreshTokenInfo> _tokens
        = new ConcurrentDictionary<string, RefreshTokenInfo>();

    // 创建并保存 refresh token
    public static string Create(string username, TimeSpan validFor)
    {
        // 生成一个 URL-safe token（更安全）
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                        .Replace("+", "-").Replace("/", "_").TrimEnd('=');

        var info = new RefreshTokenInfo
        {
            Token = token,
            Username = username,
            ExpiryDate = DateTime.UtcNow.Add(validFor)
        };

        _tokens[token] = info;
        return token;
    }

    // 验证并返回 RefreshTokenInfo（或 null）
    public static RefreshTokenInfo? Validate(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        if (_tokens.TryGetValue(token, out var info))
        {
            if (!info.IsRevoked && info.ExpiryDate > DateTime.UtcNow)
                return info;
            // 若过期或已撤销，可尝试移除以节省内存
            _tokens.TryRemove(token, out _);
        }
        return null;
    }

    // 撤销单个 token
    public static bool Revoke(string token)
    {
        if (_tokens.TryGetValue(token, out var info))
        {
            info.Revoke();
            _tokens.TryRemove(token, out _);
            return true;
        }
        return false;
    }

    // 撤销该用户的所有 refresh token（例如登出）
    public static void RevokeAllForUser(string username)
    {
        var keys = _tokens.Where(kv => kv.Value.Username == username).Select(kv => kv.Key).ToList();
        foreach (var k in keys) _tokens.TryRemove(k, out _);
    }

    // 可选：清理过期 token（可由后台定时任务调用）
    public static void RemoveExpired()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _tokens.Where(kv => kv.Value.ExpiryDate <= now).Select(kv => kv.Key).ToList();
        foreach (var k in expiredKeys) _tokens.TryRemove(k, out _);
    }
}
