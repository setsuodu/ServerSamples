// src/LeaderboardService/Data/GameDbContext.cs
using Microsoft.EntityFrameworkCore;
using LeaderboardService.Models;

namespace LeaderboardService.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

    public DbSet<ScoreDto> Scores => Set<ScoreDto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 声明为无主键实体（只读）
        modelBuilder.Entity<ScoreDto>()
            .HasNoKey()
            .ToTable("Scores", "public");

        // 可选：添加索引提升性能
        modelBuilder.Entity<ScoreDto>()
            .HasIndex(s => s.UserId);
    }
}