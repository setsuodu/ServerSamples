// src/GameService/Models/Score.cs
namespace GameService.Models;

public class Score
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public int GameId { get; set; } = 1; // 简单游戏，只有一个
    public int Points { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}