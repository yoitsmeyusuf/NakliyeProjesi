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
    public required string FullName { get; set; }

    [Required(ErrorMessage = "E-posta adresi gereklidir.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Şifre gereklidir.")]
    public required string PasswordHash { get; set; }

    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    public string PhoneNumber { get; set; } = null!;

    public bool EmailConfirmed { get; set; }

    public string? EmailConfirmationToken { get; set; }

    public string? PasswordResetToken { get; set; }

    public DateTime? PasswordResetTokenExpires { get; set; }

    [Required]
    public required UserType UserType { get; set; }

    public string? PhotoPath { get; set; }

    public ICollection<Shipment>? Shipments { get; set; }
    public ICollection<Bid>? Bids { get; set; }
    public ICollection<Rating>? Ratings { get; set; }
    public ICollection<Notification>? Notifications { get; set; }
}
