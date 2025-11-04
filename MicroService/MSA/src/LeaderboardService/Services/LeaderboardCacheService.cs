// src/LeaderboardService/Services/LeaderboardCacheService.cs
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace LeaderboardService.Services;

public class LeaderboardCacheService
{
    private readonly IDistributedCache _cache;
    private const string CacheKey = "global_leaderboard_top100";
    private readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(1);

    public LeaderboardCacheService(IDistributedCache cache) => _cache = cache;

    public async Task<List<RankItem>?> GetCachedLeaderboardAsync()
    {
        var json = await _cache.GetStringAsync(CacheKey);
        return json == null ? null : JsonSerializer.Deserialize<List<RankItem>>(json);
    }

    public async Task SetCachedLeaderboardAsync(List<RankItem> leaderboard)
    {
        var json = JsonSerializer.Serialize(leaderboard);
        await _cache.SetStringAsync(CacheKey, json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheExpiry
        });
    }
}

public class RankItem
{
    public int Rank { get; set; }
    public string UserId { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public int Score { get; set; }
}