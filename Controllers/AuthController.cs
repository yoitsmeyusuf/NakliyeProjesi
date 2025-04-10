using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NakliyeApp.Data;
using NakliyeApp.Models;
using NakliyeApp.Services;
using NakliyeApp.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("default")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly AuthService _authService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(ApplicationDbContext context, AuthService authService, IEmailService emailService, ILogger<AuthController> logger)
    {
        _context = context;
        _authService = authService;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Bu e-posta adresi zaten kullanılıyor.");

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

            var verificationLink = $"{Request.Scheme}://{Request.Host}/api/auth/verify-email?token={user.EmailConfirmationToken}";
            await _emailService.SendEmailConfirmationAsync(user, verificationLink);

            _logger.LogInformation("Kullanıcı başarıyla kaydedildi: {Email}", dto.Email);
            return Ok("Kayıt başarılı. Lütfen e-posta adresinizi doğrulayın.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kayıt sırasında bir hata oluştu.");
            return StatusCode(500, "Bir hata oluştu. Lütfen tekrar deneyin.");
        }
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailConfirmationToken == token);
            if (user == null)
                return BadRequest("Invalid token.");

            user.EmailConfirmed = true;
            user.EmailConfirmationToken = null;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Email verified for user: {UserId}", user.Id);
            return Ok("Your email has been verified.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during email verification.");
            return StatusCode(500, "An error occurred. Please try again later.");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed for email: {Email}", dto.Email);
                return Unauthorized("Invalid email or password.");
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Login attempt for unverified email: {Email}", dto.Email);
                return Unauthorized("Please verify your email first.");
            }

            var token = _authService.GenerateToken(user);
            _logger.LogInformation("User logged in successfully: {Email}", dto.Email);
            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during login for email: {Email}", dto.Email);
            return StatusCode(500, "An error occurred. Please try again later.");
        }
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return Ok("If the email is registered, a password reset link has been sent."); // Security

            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            var resetLink = $"{Request.Scheme}://{Request.Host}/reset-password?token={user.PasswordResetToken}";
            await _emailService.SendPasswordResetAsync(user, resetLink);

            _logger.LogInformation("Password reset link sent to: {Email}", dto.Email);
            return Ok("Password reset link has been sent to your email.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during forgot password.");
            return StatusCode(500, "An error occurred. Please try again later.");
        }
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == dto.Token &&
                u.PasswordResetTokenExpires > DateTime.UtcNow);

            if (user == null)
                return BadRequest("Invalid or expired token.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Password reset successfully for user: {UserId}", user.Id);
            return Ok("Your password has been updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during password reset.");
            return StatusCode(500, "An error occurred. Please try again later.");
        }
    }
}

