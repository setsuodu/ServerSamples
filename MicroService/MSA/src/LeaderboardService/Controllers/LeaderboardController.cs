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
        var result = await _db.Database
            .SqlQueryRaw<RankItem>(
                """
            SELECT 
                ROW_NUMBER() OVER (ORDER BY MAX(s."Points") DESC) as "Rank",
                s."UserId" as "UserId",
                COALESCE(u."DisplayName", '匿名') as "DisplayName",
                MAX(s."Points") as "Score"
            FROM "Scores" s
            LEFT JOIN "Users" u ON s."UserId" = u."Id"
            GROUP BY s."UserId", u."DisplayName"
            ORDER BY MAX(s."Points") DESC
            LIMIT 100
            """)
            .ToListAsync();

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