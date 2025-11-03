using Microsoft.EntityFrameworkCore;
using BugService.Models;

namespace BugService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Bug> Bugs => Set<Bug>();
}