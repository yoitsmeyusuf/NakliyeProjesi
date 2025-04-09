namespace NakliyeApp.Models;
public class Bid
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }

    public int ShipmentId { get; set; }
    public Shipment Shipment { get; set; } = null!;

    public int ShipperId { get; set; }
    public AppUser Shipper { get; set; } = null!;
}
