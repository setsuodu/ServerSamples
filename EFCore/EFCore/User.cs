using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; }

    [Required]
    [StringLength(256)]
    public string PasswordHash { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}