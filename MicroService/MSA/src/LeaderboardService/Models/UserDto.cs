// src/LeaderboardService/Models/UserDto.cs
namespace LeaderboardService.Models;

public class UserDto
{
    public string Id { get; set; } = null!;
    public string? DisplayName { get; set; }
}