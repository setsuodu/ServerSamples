// src/LeaderboardService/Data/LeaderboardDbContext.cs
using Microsoft.EntityFrameworkCore;

namespace LeaderboardService.Data;

public class LeaderboardDbContext : DbContext
{
    public LeaderboardDbContext(DbContextOptions<LeaderboardDbContext> options) : base(options) { }
}