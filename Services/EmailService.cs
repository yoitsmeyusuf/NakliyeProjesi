namespace NakliyeApp.Services;
using System.Net;
using System.Net.Mail;
using NakliyeApp.Models;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(AppUser user);
    Task SendPasswordResetAsync(AppUser user);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailConfirmationAsync(AppUser user)
    {
        var link = $"https://senin-site.com/api/auth/verify-email?token={user.EmailConfirmationToken}";
        var subject = "E-posta Doğrulama";
        var body = $"""
            Merhaba {user.FullName},<br/>
            Lütfen e-posta adresinizi doğrulamak için <a href="{link}">buraya tıklayın</a>.
        """;

        await SendEmailAsync(user.Email, subject, body);
    }

    public async Task SendPasswordResetAsync(AppUser user)
    {
        var link = $"https://senin-site.com/reset-password?token={user.PasswordResetToken}";
        var subject = "Şifre Sıfırlama";
        var body = $"""
            Merhaba {user.FullName},<br/>
            Şifrenizi sıfırlamak için <a href="{link}">buraya tıklayın</a>. Link 1 saat geçerlidir.
        """;

        await SendEmailAsync(user.Email, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var smtpClient = new SmtpClient(_config["Smtp:Host"])
        {
            Port = int.Parse(_config["Smtp:Port"]),
            Credentials = new NetworkCredential(_config["Smtp:Username"], _config["Smtp:Password"]),
            EnableSsl = true,
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_config["Smtp:From"]),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(toEmail);

        try
        {
            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mail gönderme hatası");
        }
    }
}
