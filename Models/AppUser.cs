namespace NakliyeApp.Models;
public enum UserType
{
    Shipper,     // Nakliyeci
    Customer     // İlan Veren
}

public class AppUser
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public string EmailConfirmationToken { get; set; }
    public string PasswordResetToken { get; set; }

    public DateTime? PasswordResetTokenExpires { get; set; }
    public UserType UserType { get; set; }

    // Navigation properties
    public ICollection<Shipment>? Shipments { get; set; } // Eğer müşteri ise
    public ICollection<Bid>? Bids { get; set; }           // Eğer nakliyeci ise
}
