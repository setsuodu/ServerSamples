// src/LeaderboardService/Data/UserDbContext.cs
using Microsoft.EntityFrameworkCore;
using LeaderboardService.Models;

namespace LeaderboardService.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<UserDto> Users => Set<UserDto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserDto>()
            .HasNoKey()
            .ToTable("Users", "public");

        modelBuilder.Entity<UserDto>()
            .HasIndex(u => u.Id);
    }
}