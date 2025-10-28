namespace EFCore.Models
{
    // Models/BanRecord.cs
    public class BanRecord
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long? AdminId { get; set; }
        public string Reason { get; set; } = null!;
        public DateTime BannedAt { get; set; }
        public DateTime? BannedUntil { get; set; }
        public bool IsActive { get; set; } = true;

        public User User { get; set; } = null!;
        public User? Admin { get; set; }
    }
}