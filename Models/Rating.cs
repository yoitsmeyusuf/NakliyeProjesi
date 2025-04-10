using System.ComponentModel.DataAnnotations;

namespace NakliyeApp.Models;

public class Rating
{
    public int Id { get; set; }

    [Required]
    public required int UserId { get; set; }
    public required AppUser User { get; set; } = null!;

    [Required]
    public required int ShipmentId { get; set; } // Link to the shipment being rated

    [Required]
    public required int CreatedByUserId { get; set; } // User who created the rating

    [Required]
    [Range(1, 5, ErrorMessage = "Puan 1 ile 5 arasında olmalıdır.")]
    public required int Score { get; set; }

    public string? Comment { get; set; }
}
