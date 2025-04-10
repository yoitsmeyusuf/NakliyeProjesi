using System.ComponentModel.DataAnnotations;

namespace NakliyeApp.Models;
public enum UserType
{
    Shipper,     // Nakliyeci
    Customer     // İlan Veren
}

public class AppUser
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ad ve soyad gereklidir.")]
    [MaxLength(100, ErrorMessage = "Ad ve soyad en fazla 100 karakter olabilir.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "E-posta adresi gereklidir.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Şifre gereklidir.")]
    public string PasswordHash { get; set; } = null!;

    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    public string PhoneNumber { get; set; } = null!;

    public bool EmailConfirmed { get; set; }

    public string? EmailConfirmationToken { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetTokenExpires { get; set; }

    [Required]
    public UserType UserType { get; set; }

    public string? PhotoPath { get; set; } // Path to the profile photo

    // Navigation properties
    public ICollection<Shipment>? Shipments { get; set; } // Eğer müşteri ise
    public ICollection<Bid>? Bids { get; set; }           // Eğer nakliyeci ise
    public ICollection<Rating>? Ratings { get; set; }     // User ratings
    public ICollection<Notification>? Notifications { get; set; } // Notifications for the user
}
