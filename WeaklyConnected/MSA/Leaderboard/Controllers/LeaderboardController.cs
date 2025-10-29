// src/LeaderboardService/Controllers/LeaderboardController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeaderboardService.Data;
using LeaderboardService.Services;

namespace LeaderboardService.Controllers;

[ApiController]
[Route("api/leaderboard")]
public class LeaderboardController : ControllerBase
{
    private readonly LeaderboardDbContext _db;
    private readonly LeaderboardCacheService _cache;

    public LeaderboardController(LeaderboardDbContext db, LeaderboardCacheService cache)
    {
        _db = db;
        _cache = cache;
    }

    [HttpGet("global")]
    public async Task<IActionResult> GetGlobalTop100()
    {
        // 1. 查缓存
        var cached = await _cache.GetCachedLeaderboardAsync();
        if (cached != null)
            return Ok(cached);

        // 2. 查数据库
        var top100 = await _db.Scores
            .FromSqlRaw("""
                SELECT 
                    s."UserId", 
                    u."DisplayName", 
                    MAX(s."Points") as "Points"
                FROM "Scores" s
                JOIN "Users" u ON s."UserId" = u."Id"
                GROUP BY s."UserId", u."DisplayName"
                ORDER BY MAX(s."Points") DESC
                LIMIT 100
                """)
            .ToListAsync();

        var result = top100.Select((x, i) => new RankItem
        {
            Rank = i + 1,
            UserId = x.UserId,
            DisplayName = x.DisplayName ?? "匿名",
            Score = x.Points
        }).ToList();

        // 3. 写缓存
        await _cache.SetCachedLeaderboardAsync(result);

        return Ok(result);
    }

    [HttpGet("my-rank")]
    [Authorize]
    public async Task<IActionResult> GetMyRank()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        var rankResult = await _db.Set<UserRankDto>()
            .FromSqlRaw("""
            SELECT RANK() OVER (ORDER BY MAX(s."Points") DESC) AS rank
            FROM "Scores" s
            WHERE s."UserId" = {0}
            GROUP BY s."UserId"
            """, userId)
            .FirstOrDefaultAsync();

        var rank = rankResult?.rank ?? 0;

        return Ok(new { UserId = userId, Rank = (int)rank });
    }
}