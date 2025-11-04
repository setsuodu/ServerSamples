// src/LeaderboardService/Models/ScoreDto.cs
namespace LeaderboardService.Models;

public class ScoreDto
{
    public string UserId { get; set; } = null!;
    public int Points { get; set; }
}