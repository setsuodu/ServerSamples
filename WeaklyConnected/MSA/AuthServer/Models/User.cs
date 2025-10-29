namespace AuthServer.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        public bool IsActive { get; set; } = true;
        public bool IsBanned { get; set; } = false;
        public DateTime? BannedUntil { get; set; }
        public string? BannedReason { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
