// Data/GameDbContext.cs
using Microsoft.EntityFrameworkCore;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<LoginLog> LoginLogs => Set<LoginLog>();
    public DbSet<ThirdPartyLogin> ThirdPartyLogins => Set<ThirdPartyLogin>();
    public DbSet<BanRecord> BanRecords => Set<BanRecord>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 全局 snake_case 命名（保留）
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            entity.SetTableName(entity.GetTableName()!.ToSnakeCase());

            foreach (var property in entity.GetProperties())
                property.SetColumnName(property.GetColumnName().ToSnakeCase());

            foreach (var key in entity.GetKeys())
                key.SetName(key.GetName()!.ToSnakeCase());

            foreach (var fk in entity.GetForeignKeys())
                fk.SetConstraintName(fk.GetConstraintName()!.ToSnakeCase());
        }

        // === 手动配置 BanRecord.Admin 关系 ===
        modelBuilder.Entity<BanRecord>(entity =>
        {
            entity.HasOne(br => br.Admin)
                  .WithMany()  // Admin 没有反向导航集合，所以用 WithMany()
                  .HasForeignKey(br => br.AdminId)
                  .OnDelete(DeleteBehavior.SetNull)  // 可空外键，删除时设为 NULL
                  .HasConstraintName("fk_ban_records_admin_id");
        });

        // === 其他唯一索引 ===
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Character>()
            .HasIndex(c => new { c.UserId, c.Name })
            .IsUnique();

        modelBuilder.Entity<ThirdPartyLogin>()
            .HasIndex(t => new { t.Provider, t.ProviderId })
            .IsUnique();

        // Friend 表配置
        modelBuilder.Entity<Friend>(entity =>
        {
            entity.ToTable("friends");

            entity.HasKey(f => f.Id);

            entity.HasIndex(f => new { f.UserId, f.FriendUserId })
                  .IsUnique();

            entity.HasIndex(f => f.FriendUserId);
            entity.HasIndex(f => f.Status);
            entity.HasIndex(f => f.CreatedAt);

            // 关系配置：User (1) - (*) Friend
            entity.HasOne(f => f.User)
                  .WithMany(u => u.Friends)
                  .HasForeignKey(f => f.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(f => f.FriendUser)
                  .WithMany(u => u.FriendOf)
                  .HasForeignKey(f => f.FriendUserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 软删除过滤
        modelBuilder.Entity<User>().HasQueryFilter(u => u.DeletedAt == null);
    }
}

// 扩展方法：PascalCase → snake_case
public static class StringExtensions
{
    public static string ToSnakeCase(this string str)
    {
        return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()))
                     .ToLower();
    }
}