using System.ComponentModel.DataAnnotations;

namespace NakliyeApp.Models;

public enum ShipmentStatus
{
    Active,
    Completed,
    Cancelled
}

public class Shipment
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = null!;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string PickupLocation { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string DeliveryLocation { get; set; } = null!;

    [Required]
    public DateTime PickupDate { get; set; }

    public int? AcceptedBidId { get; set; }
    public Bid? AcceptedBid { get; set; }

    public bool IsCompleted { get; set; } = false;

    [Required]
    public int CustomerId { get; set; }
    public AppUser Customer { get; set; } = null!;

    public ICollection<Bid> Bids { get; set; } = new List<Bid>();

    [Required]
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Active;

    public string? PhotoPath { get; set; } // Path to the shipment photo
}
