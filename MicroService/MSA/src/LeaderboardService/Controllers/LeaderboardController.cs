// src/LeaderboardService/Controllers/LeaderboardController.cs
using LeaderboardService.Data;
using LeaderboardService.Models;
using LeaderboardService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaderboardService.Controllers;

[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController : ControllerBase
{
    private readonly GameDbContext _db;
    private readonly LeaderboardCacheService _cache;

    public LeaderboardController(GameDbContext db, LeaderboardCacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    [HttpGet("global")]
    public async Task<IActionResult> GetGlobalTop100(
    [FromServices] GameDbContext gameDb,
    [FromServices] UserDbContext userDb,
    [FromServices] LeaderboardCacheService cache)
    {
        Console.WriteLine("请求了全球排名");

        var cached = await cache.GetCachedLeaderboardAsync();
        if (cached != null) return Ok(cached);

        var topScores = await gameDb.Scores
            .GroupBy(s => s.UserId)
            .Select(g => new { g.Key, Score = g.Max(s => s.Points) })
            .OrderByDescending(x => x.Score)
            .Take(100)
            .ToListAsync();

        var userIds = topScores.Select(x => x.Key).ToList();
        var users = await userDb.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.DisplayName ?? "匿名");

        var result = topScores.Select((x, i) => new RankItem
        {
            Rank = i + 1,
            UserId = x.Key,
            DisplayName = users.GetValueOrDefault(x.Key, "匿名"),
            Score = x.Score
        }).ToList();

        await cache.SetCachedLeaderboardAsync(result);
        return Ok(result);
    }

    [HttpGet("my-rank")]
    [Authorize]
    public async Task<IActionResult> GetMyRank()
    {
        Console.WriteLine("请求了我的排名");

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        // 直接执行 SQL，返回标量
        var rank = await _db.Database
            .SqlQueryRaw<RankResult>(
                """
        SELECT RANK() OVER (ORDER BY MAX(s."Points") DESC)::int AS "Value"
        FROM "Scores" s
        WHERE s."UserId" = {0}
        GROUP BY s."UserId"
        """, userId)
            .Select(r => r.Value)
            .FirstOrDefaultAsync();

        return Ok(new { UserId = userId, Rank = rank });
    }
}