namespace NakliyeApp.DTOs;
public class RegisterDto
{
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string PhoneNumber { get; set; }
    public int UserType { get; set; } // 0: Müşteri, 1: Nakliyeci
}