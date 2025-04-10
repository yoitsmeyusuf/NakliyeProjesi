using System.ComponentModel.DataAnnotations;

namespace NakliyeApp.DTOs;

public class BidDto
{
    [Required(ErrorMessage = "Teklif fiyatı gereklidir.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Teklif fiyatı sıfırdan büyük olmalıdır.")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "İlan ID gereklidir.")]
    public int ShipmentId { get; set; }
}
