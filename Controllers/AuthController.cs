using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NakliyeApp.Data;
using NakliyeApp.Models;
using NakliyeApp.Services;
using NakliyeApp.DTOs;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AuthService _authService;
    private readonly IEmailService _emailService;

    public AuthController(ApplicationDbContext context, AuthService authService, IEmailService emailService)
    {
        _context = context;
        _authService = authService;
        _emailService = emailService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Bu e-posta zaten kullanılıyor.");

        var user = new AppUser
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            PhoneNumber = dto.PhoneNumber,
            UserType = (UserType)dto.UserType,
            EmailConfirmed = false,
            EmailConfirmationToken = Guid.NewGuid().ToString(),
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await _emailService.SendEmailConfirmationAsync(user);

        return Ok("Kayıt başarılı. Lütfen e-posta adresinizi doğrulayın.");
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailConfirmationToken == token);
        if (user == null)
            return BadRequest("Geçersiz token.");

        user.EmailConfirmed = true;
        user.EmailConfirmationToken = null;
        await _context.SaveChangesAsync();

        return Ok("E-posta adresiniz doğrulandı.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("E-posta veya şifre hatalı.");

        if (!user.EmailConfirmed)
            return Unauthorized("Lütfen önce e-posta adresinizi doğrulayın.");

        var token = _authService.GenerateToken(user);
        return Ok(new { Token = token });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null)
            return Ok("Eğer e-posta sistemde kayıtlıysa şifre sıfırlama bağlantısı gönderildi."); // Güvenlik

        user.PasswordResetToken = Guid.NewGuid().ToString();
        user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);
        await _context.SaveChangesAsync();

        await _emailService.SendPasswordResetAsync(user);

        return Ok("Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.");
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.PasswordResetToken == dto.Token &&
            u.PasswordResetTokenExpires > DateTime.UtcNow);

        if (user == null)
            return BadRequest("Geçersiz veya süresi dolmuş token.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpires = null;

        await _context.SaveChangesAsync();

        return Ok("Şifreniz başarıyla güncellendi.");
    }
}

