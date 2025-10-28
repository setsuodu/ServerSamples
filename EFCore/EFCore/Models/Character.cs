// Models/Character.cs
public class Character
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Name { get; set; } = null!;
    public int Level { get; set; } = 1;
    public long Exp { get; set; } = 0;
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}