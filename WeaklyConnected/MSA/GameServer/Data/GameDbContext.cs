// src/GameService/Data/GameDbContext.cs
using Microsoft.EntityFrameworkCore;
using GameService.Models;

namespace GameService.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

    public DbSet<Score> Scores => Set<Score>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Score>()
            .HasIndex(s => s.UserId);
        modelBuilder.Entity<Score>()
            .HasIndex(s => s.Points);
    }
}