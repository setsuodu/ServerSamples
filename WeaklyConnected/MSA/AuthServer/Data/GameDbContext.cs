using Microsoft.EntityFrameworkCore;
using AuthServer.Models;
//using AuthServer.Extensions;

namespace AuthServer.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>(); // 演示 CRUD
    }
}
