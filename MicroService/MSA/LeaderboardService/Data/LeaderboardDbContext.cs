// src/LeaderboardService/Data/LeaderboardDbContext.cs
using Microsoft.EntityFrameworkCore;

namespace LeaderboardService.Data;

public class LeaderboardDbContext : DbContext
{
    public LeaderboardDbContext(DbContextOptions<LeaderboardDbContext> options) : base(options) { }

    // 使用 GameService 的 Scores 表（共享数据库）
    public DbSet<ScoreDto> Scores => Set<ScoreDto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 告诉 EF Core：ScoreDto 是一个“无键实体”（Keyless Entity）
        modelBuilder.Entity<ScoreDto>().HasNoKey();
    }
}

public class ScoreDto
{
    public string UserId { get; set; } = null!;
    public string? DisplayName { get; set; }
    public int Points { get; set; }
}