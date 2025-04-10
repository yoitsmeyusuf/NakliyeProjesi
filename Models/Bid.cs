using System.ComponentModel.DataAnnotations;

namespace NakliyeApp.Models;
public class Bid
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Teklif fiyatı gereklidir.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Teklif fiyatı sıfırdan büyük olmalıdır.")]
    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }

    [Required]
    public int ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;

    [Required]
    public int ShipperId { get; set; }
    public AppUser Shipper { get; set; } = null!;

    public int? Rating { get; set; } // Optional rating for the bid
}
