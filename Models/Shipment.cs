namespace NakliyeApp.Models;
public class Shipment
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string PickupLocation { get; set; } = null!;
    public string DeliveryLocation { get; set; } = null!;
    public DateTime PickupDate { get; set; }

    public int? AcceptedBidId { get; set; }
    public Bid? AcceptedBid { get; set; }
    public bool IsCompleted { get; set; } = false;

    public int CustomerId { get; set; }
    public AppUser Customer { get; set; } = null!;

    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
}
