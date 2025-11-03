// src/GameService/Controllers/ScoreController.cs
using GameService.Data;
using GameService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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
        Console.WriteLine("收到分数提交请求");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
            return BadRequest(new { Message = "验证失败", Errors = errors });
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("无效 Token");

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
    //public string UserId { get; set; } = null!;
    //public string? DisplayName { get; set; }
    [Required]
    [Range(0, 1_000_000)]
    public int Points { get; set; }
}