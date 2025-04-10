using System.ComponentModel.DataAnnotations;

namespace NakliyeApp.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "Ad ve soyad gereklidir.")]
    [MaxLength(100, ErrorMessage = "Ad ve soyad en fazla 100 karakter olabilir.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "E-posta adresi gereklidir.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Şifre gereklidir.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    public string Password { get; set; } = null!;

    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "Kullanıcı türü gereklidir.")]
    public int UserType { get; set; }
}