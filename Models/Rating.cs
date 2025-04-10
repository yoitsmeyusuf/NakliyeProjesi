using System.ComponentModel.DataAnnotations;

namespace NakliyeApp.Models;

public class Rating
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    [Required]
    public int ShipmentId { get; set; } // Link to the shipment being rated

    [Required]
    public int CreatedByUserId { get; set; } // User who created the rating

    [Required]
    [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
    public int Score { get; set; }

    public string? Comment { get; set; }
}
