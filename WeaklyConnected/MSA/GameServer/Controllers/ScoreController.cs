// src/GameService/Controllers/ScoreController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GameService.Data;
using GameService.Models;

namespace GameService.Controllers;

[ApiController]
[Route("api/game")]
[Authorize]
public class ScoreController : ControllerBase
{
    private readonly GameDbContext _db;

    public ScoreController(GameDbContext db) => _db = db;

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitScore([FromBody] SubmitScoreDto dto)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        // 简单防作弊：分数范围
        if (dto.Points < 0 || dto.Points > 1_000_000) return BadRequest("无效分数");

        var score = new Score
        {
            UserId = userId,
            Points = dto.Points
        };

        _db.Scores.Add(score);
        await _db.SaveChangesAsync();

        return Ok(new { Message = "提交成功", ScoreId = score.Id });
    }
}

public class SubmitScoreDto
{
    public string UserId { get; set; } = null!;
    public string? DisplayName { get; set; }
    public int Points { get; set; }
}