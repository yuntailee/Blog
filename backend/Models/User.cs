using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogApi.Models;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string TotpSecret { get; set; } = string.Empty; // Encrypted TOTP Secret

    [MaxLength(100)]
    public string? Email { get; set; }

    [Column(TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "timestamp with time zone")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
