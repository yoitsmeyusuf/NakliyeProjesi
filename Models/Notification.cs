using System.ComponentModel.DataAnnotations;

namespace NakliyeApp.Models;

public class Notification
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsRead { get; set; } = false;
}
